
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
   
    public class Chat
    {
        
        public string Name{get;set;}
        
        public string ?Id{get;set;}
        
        public List<User> Participants{get;set;}
        
        public Chat()
        {
            Participants = new List<User>();
        }
    }
        
}