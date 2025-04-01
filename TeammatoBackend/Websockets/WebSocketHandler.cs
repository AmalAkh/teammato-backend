
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.WebSockets;
using System.Text;
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
   
    public class WebSocketHandler
    {
        
        Dictionary<string, List<WebSocket>> connectedUsers = new Dictionary<string, List<WebSocket>>();


        public async Task HandleConnection(HttpContext httpContext)
        {
            if(httpContext.WebSockets.IsWebSocketRequest)
            {
                var ws = await httpContext.WebSockets.AcceptWebSocketAsync();
                
                CancellationTokenSource authCancellationTokenSource = new CancellationTokenSource();

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
                if(validationResult.IsValid)
                {
                    await ws.SendAsync(Encoding.UTF8.GetBytes("success"), WebSocketMessageType.Text,true, CancellationToken.None);
                }else
                {
                    await ws.SendAsync(Encoding.UTF8.GetBytes("failed"), WebSocketMessageType.Text,true, CancellationToken.None);
                    return;
                }
                string userId = (string)validationResult.Claims["UserId"];
                if (!connectedUsers.TryGetValue(userId, out var sockets))
                {
                    sockets = new List<WebSocket>();
                    connectedUsers[userId] = sockets;
                }
                sockets.Add(ws);
                
                await StartListeningWebSocketMessages(ws, userId);
       

            }
        }
        
        public async Task StartListeningWebSocketMessages(WebSocket ws, string userId)
        {
            CancellationTokenSource  receivingCancellationTokenSource = new CancellationTokenSource();
            DateTime lastMessageTime = DateTime.UtcNow;
            System.Timers.Timer timer = new System.Timers.Timer(2500);
            timer.AutoReset = true;
            timer.Elapsed+= async delegate
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