
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
    public enum WebSocketNotificationType
    {
        NewPlayerJoinedNotification, PlayerLeavedGameSession, NewChatMessage
    }
    public static class WebSocketNotificationFactory
    {
        public static WebSocketNotification CreateNotification<T>(WebSocketNotificationType messageType, T content)
        {
            var newMessage = new WebSocketNotification();
            newMessage.Type = messageType;
            
            newMessage.Content = JsonSerializer.Serialize<T>(content);
            return newMessage;
        }

    }
        
}