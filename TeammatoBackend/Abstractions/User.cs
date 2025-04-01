
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TeammatoBackend.Abstractions
{
   
    public class User
    {
        
        public string NickName{get;set;}
        
        
        public string Email{get;set;}
        

        public string Password{get;set;}
       

        public string Image{get;set;}
        [Key]
       

        public string ?Id{get;set;}
        
        public ICollection<Language> ?Languages {get;set;}
        [JsonIgnore]
        public ICollection<Chat> ?Chats {get;set;}
    }
}