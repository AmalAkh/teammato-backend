using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TeammatoBackend.Abstractions
{
    public class Message
    {
        public string ?Id { get; set; } = Guid.NewGuid().ToString();
        public string ?ChatId { get; set; }   
        public string ?UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Content { get; set; }
        [JsonIgnore]
        public User ?Sender { get; set; }
        [JsonIgnore]
        public Chat ?Chat { get; set; }
    }
}