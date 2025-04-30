
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
    // WebSocket service class that uses WebSocketHandler to manage connections and notifications
    public static class WebSocketService
    {
        // Static instance of WebSocketHandler to handle WebSocket connections
        static WebSocketHandler webSocketHandler = new WebSocketHandler();
        // Method to notify all users in a game session, including the owner
        public static async Task NotifyBySession(GameSession gameSession, WebSocketNotification notification)
        {
            // Send notification to the game session owner
            await webSocketHandler.SendMessageTo(gameSession.Owner.Id, notification);
            // Send notification to all users in the game session
            foreach(string userId in gameSession.Users.Keys)
            {
                await webSocketHandler.SendMessageTo(userId, notification);
            }
        }
        

        public static async Task HandleConnection(HttpContext httpContext)
        {
            await webSocketHandler.HandleConnection(httpContext);
        }
    }
        
}