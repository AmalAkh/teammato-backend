
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TeammatoBackend.Abstractions;
using TeammatoBackend.Utils;

namespace TeammatoBackend.WebSockets
{
    // Represents a WebSocket notification with a type and content
    public class WebSocketNotification
    {
        public WebSocketNotificationType Type {get;set;}
        public string Content {get;set;}
    }
    // Handles WebSocket connections and communication
    public class WebSocketHandler
    {
        // Dictionary to store connected users and their corresponding WebSocket connections
        Dictionary<string, List<WebSocket>> connectedUsers = new Dictionary<string, List<WebSocket>>();
        // Sends a message to a specific user via WebSocket
        public async Task SendMessageTo(string targetUserId,WebSocketNotification message)
        {
            if(!connectedUsers.ContainsKey(targetUserId))
            {
                return;
            }
            foreach(WebSocket ws in  connectedUsers[targetUserId])
            {
                string jsonString = JsonSerializer.Serialize(message);
                await ws.SendAsync(Encoding.UTF8.GetBytes(jsonString), WebSocketMessageType.Text, true, CancellationToken.None);
           
            }       
        }
        // Handles incoming WebSocket connection requests
        public async Task HandleConnection(HttpContext httpContext)
        {
            if(httpContext.WebSockets.IsWebSocketRequest)
            {
                var ws = await httpContext.WebSockets.AcceptWebSocketAsync();
                
                CancellationTokenSource authCancellationTokenSource = new CancellationTokenSource();
                // Timer to close the connection if not authenticated within 10 seconds
                System.Timers.Timer timer = new System.Timers.Timer(10000);
                timer.AutoReset = false;
                timer.Elapsed+= async delegate
                {
                    authCancellationTokenSource.Cancel();
                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
                    }
                
                };
                
                byte[] buffer = new byte[1024];
                timer.Start();
                
                WebSocketReceiveResult receiveResult;
                try
                {
                    receiveResult = await ws.ReceiveAsync(buffer, authCancellationTokenSource.Token);
                    timer.Stop();
                }catch(OperationCanceledException)
                {
                    return;   
                }
                
                // Validate the token sent by the user
                var token = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                var validator = new JsonWebTokenHandler();
                TokenValidationResult validationResult = await validator.ValidateTokenAsync(token,  new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtAuthOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = JwtAuthOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = JwtAuthOptions.GetAccessTokenSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true,
                });
                // Send success or failure message based on token validation result
                if(validationResult.IsValid)
                {
                    await ws.SendAsync(Encoding.UTF8.GetBytes("success"), WebSocketMessageType.Text,true, CancellationToken.None);
                }else
                {
                    await ws.SendAsync(Encoding.UTF8.GetBytes("failed"), WebSocketMessageType.Text,true, CancellationToken.None);
                    return;
                }
                // Add user to connected users list
                string userId = (string)validationResult.Claims["UserId"];
                if (!connectedUsers.TryGetValue(userId, out var sockets))
                {
                    sockets = new List<WebSocket>();
                    connectedUsers[userId] = sockets;
                }
                sockets.Add(ws);
                // Start listening for messages from the WebSocket
                await StartListeningWebSocketMessages(ws, userId);
       

            }
        }
        // Listens for incoming messages from a specific WebSocket
        public async Task StartListeningWebSocketMessages(WebSocket ws, string userId)
        {
            CancellationTokenSource  receivingCancellationTokenSource = new CancellationTokenSource();
            DateTime lastMessageTime = DateTime.UtcNow;
            // Timer to check WebSocket status and remove it if disconnected
            System.Timers.Timer timer = new System.Timers.Timer(2500);
            timer.AutoReset = true;
            timer.Elapsed +=  delegate
            {
                if(ws.State != WebSocketState.Open)
                {
                    if (!connectedUsers.TryGetValue(userId, out var sockets))
                    {
                        sockets = new List<WebSocket>();
                        connectedUsers[userId] = sockets;
                    }
                    sockets.Remove(ws);
                    timer.Stop(); 
                }
                
            
            };
            timer.Start();

            
            while(true)
            {
                
                
                byte[] buffer = new byte[1024];
                try
                {
                    await ws.ReceiveAsync(buffer, receivingCancellationTokenSource.Token);
                }catch(WebSocketException)
                {
                    return;
                }

                
            }
        }
        

    }
        
}