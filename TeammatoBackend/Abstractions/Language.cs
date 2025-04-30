
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
    // This class represents a language that a user speaks or knows.
    // It stores information such as the language's ISO name (e.g., "en" for English) and the user who knows that language.
    public class Language
    {
        // The ISO name of the language.
        // This is a standard code for the language (e.g., "en" for English, "es" for Spanish).
        public string ISOName{get;set;}

        // The User ID of the user who speaks the language.
        // This establishes a relationship between the user and the language they know.
        public string ?UserId{get;set;}

        // Navigation property to the User entity.
        // It links the language to a specific user who speaks it.
        // The [JsonIgnore] attribute ensures this property is excluded from serialization to JSON,
        // preventing circular references and potential issues when sending data over the API.
        [JsonIgnore]
        public User ?User{get;set;}

    }
        
}