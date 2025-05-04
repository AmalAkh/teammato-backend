
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.Abstractions
{
    // This class represents a Chat entity in the system, including information about the chat's name,
    // its participants, and the messages exchanged within the chat.
    public class Chat
    {
        // The Name of the chat.
        public string Name{get;set;}
        
        // The unique identifier for the chat.
        public string ?Id{get;set;}

        public string ?Image{get;set;}
        
        // A list of participants in the chat.
        public List<User> Participants{get;set;}

        // A list of messages associated with the chat.
        public List<Message> Messages{get;set;}
        
        
        // Constructor for the Chat class that initializes the Participants and Messages lists.
        // The lists are initialized to avoid null references and to provide an empty list by default.
        public Chat()
        {
            // Initializes an empty list of Participants, which will store all users in the chat.
            Participants = new List<User>();
            // Initializes an empty list of Messages, which will store all the messages exchanged in the chat.
            Messages = new List<Message>();
        }
    }
        
}