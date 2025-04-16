using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TeammatoBackend.Abstractions
{
    // This class represents a message sent within a specific chat by a user.
    public class Message
    {
        // Unique identifier for the message.
        // A new GUID is generated if no ID is provided, ensuring each message has a distinct identifier.
        public string ?Id { get; set; } = Guid.NewGuid().ToString();

        // The ID of the chat in which the message was sent.
        // This links the message to a specific chat.
        public string ?ChatId { get; set; }   

        // The ID of the user who sent the message.
        // This helps identify the sender of the message.
        public string ?UserId { get; set; }

        // The date and time the message was created (in UTC).
        // This is automatically set to the current time when a new message is created.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // The content of the message.
        public string Content { get; set; }

        public bool IsEdited {get; set;} = false;

        // Navigation property for the sender (User) of the message.
        // The [JsonIgnore] attribute ensures this property is excluded from JSON serialization to avoid circular references
        // and excessive data in the API response.
        [JsonIgnore]
        public User ?Sender { get; set; }

        // Navigation property for the associated chat.
        // This is the chat where the message belongs.
        // The [JsonIgnore] attribute prevents it from being serialized, ensuring only relevant data is sent in the API response.
        [JsonIgnore]
        public Chat ?Chat { get; set; }
    }
}