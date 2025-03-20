
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace TeammatoBackend.Abstractions
{
   
    public class User
    {
        
        public string Name{get;set;}
        

        public string Email{get;set;}
        

        public string Password{get;set;}
       

        public string Image{get;set;}
        [Key]
       

        public string ?Id{get;set;}
    }
}