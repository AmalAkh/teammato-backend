
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
    // Enum to define different types of WebSocket notifications
    public enum WebSocketNotificationType
    {
        NewPlayerJoined, PlayerLeavedGameSession, NewChatMessage, GameSessionStarted, SuccessAuth, GameSessionCancelled
    }
    // Factory class to create WebSocket notifications
    public static class WebSocketNotificationFactory
    {
        // Method to create a WebSocket notification with specific type and content
        public static WebSocketNotification CreateNotification<T>(WebSocketNotificationType messageType, T content)
        {
            var newMessage = new WebSocketNotification();
            newMessage.Type = messageType; // Set the notification type
            newMessage.Content = JsonSerializer.Serialize<T>(content, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }); // Serialize content to JSON
            return newMessage;
        }

    }
        
}