
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
   
    public static class WebSocketService
    {
        static WebSocketHandler webSocketHandler = new WebSocketHandler();

        public static async Task NotifyBySession(GameSession gameSession, WebSocketNotification notification)
        {
            await webSocketHandler.SendMessageTo(gameSession.Owner.Id, notification);
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