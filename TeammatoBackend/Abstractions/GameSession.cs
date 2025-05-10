
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using IGDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TeammatoBackend.Abstractions;
namespace TeammatoBackend.Abstractions
{
    
    public class GameSession
    {
        [Key]
        public string Id { get; set; }

        public string GameId { get; set; }

        public string GameName { get; set; }

        public string ?Description { get; set; }

        public string Image { get; set; }

        public uint RequiredPlayersCount { get; set; }

        public User Owner { get; set; }

        public double Latitude{get;set;}
        public double Longitude{get;set;}
        public double Duration{get;set;}


        public string Langs {get;set;}


        public List<User>? Participants { get; set; }


        public GameSession()
        {
            Participants = new List<User>();
        }
    }

}