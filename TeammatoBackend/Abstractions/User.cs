
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace TeammatoBackend.Abstractions
{
    [Table("users")]
    public class User
    {
        [Column("name")]
        public string ?Name{get;set;}
        [Column("email")]

        public string Email{get;set;}
        [Column("password")]

        public string Password{get;set;}
        [Column("image")]

        public string Image{get;set;}
        [Key]
        [Column("id")]

        public string ?Id{get;set;}
    }
}