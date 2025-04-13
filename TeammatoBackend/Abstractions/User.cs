
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TeammatoBackend.Abstractions
{
    // This class represents a user within the application.
    public class User
    {
        // The user's nickname (username).
        public string NickName{get;set;}
        
        // The user's email address, used for authentication and notifications.
        public string Email{get;set;}
        
        // The user's password, stored securely and used for authentication.
        public string Password{get;set;}
       
        // The user's profile image.
        public string ?Image{get;set;}
        
        // The user's unique identifier.
        [Key]
        public string ?Id{get;set;}

        // A collection of languages that the user is associated with.
        public ICollection<Language> ?Languages {get;set;}

        // A collection of messages that this user has sent.
        public ICollection<Message> ?Messages {get;set;}

        // A collection of favorite games associated with this user.
        [JsonIgnore]
        public ICollection<FavoriteGame> ?FavoriteGames {get;set;}

        // A collection of chats that this user is a participant in.
        [JsonIgnore]
        public ICollection<Chat> ?Chats {get;set;}
    }
}