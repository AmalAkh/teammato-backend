
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
    // This class represents a user's favorite game.
    // It associates a user with a game they have marked as a favorite, 
    // and stores game details such as name and image.
    public class  FavoriteGame
    {
        // The unique identifier of the game (Game ID).
        public string GameId {get;set;}

        // The User ID of the user who marked the game as a favorite.
        // This establishes a relationship between the user and the game they like.
        public string ?UserId {get;set;}

        // The name of the game.
        // This could be the title of the game, like "The Witcher 3" or "League of Legends".
        public string Name {get;set;}

        // The image URL or path for the game's cover image.
        public string Image {get;set;}

        // Navigation property to link to the User entity.
        // It represents the user who has marked the game as a favorite.
        // The [JsonIgnore] attribute ensures this property is excluded when serializing to JSON, 
        // preventing circular references and potential issues when sending data to the client.
        [JsonIgnore]
        public User ?User{get;set;}



    }
        
}