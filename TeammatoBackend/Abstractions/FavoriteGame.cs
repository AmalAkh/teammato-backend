
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
   
    public class  FavoriteGame
    {
        
        public string GameId {get;set;}
        public string ?UserId {get;set;}


        public string Name {get;set;}
        public string Image {get;set;}
        [JsonIgnore]
        public User ?User{get;set;}



    }
        
}