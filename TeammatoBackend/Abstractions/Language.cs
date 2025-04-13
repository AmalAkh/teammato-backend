
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
   
    public class Language
    {
        
        public string ISOName{get;set;}
        
        public string ?UserId{get;set;}
        [JsonIgnore]
        public User ?User{get;set;}

    }
        
}