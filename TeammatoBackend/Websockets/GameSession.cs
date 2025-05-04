
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TeammatoBackend.Abstractions;
namespace TeammatoBackend.WebSockets
{
    
    public class GameSession
    {
        // Protected fields for game session properties
        protected string description;
        public string Description
        {
            get
            {
                return description; // Read-only property for description
            }
        }
        protected string gameName;
        public string GameName
        {
            get
            {
                return gameName; // Read-only property for game name
            }
        }
        protected string image;
        public string Image
        {
            get
            {
                return image; // Read-only property for image ID
            }
        }
        protected string id;
        public string Id
        {
            get
            {
                return id; // Read-only property for session ID
            }
        }
        public string GameId
        {
            get
            {
                return gameId; // Read-only property for game ID
            }
        }

        protected string gameId;


        public uint RequiredPlayersCount
        {
            get
            {
                return requiredPlayersCount; // Read-only property for required player count
            }
        }

        protected uint requiredPlayersCount;

        protected User owner;
        public User Owner
        {
            get
            {
                return owner; // Read-only property for the owner
            }
        }
        protected Dictionary<string, User> users;

        public Dictionary<string,User> Users
        {
            get{return users;} // Read-only property for list of users
        }

        // Method to add a user to the session
        public void Join(User user)
        {
            users.Add(user.Id, user);
        }

        // Method to remove a user from the session
        public bool Leave(User user)
        {
            return users.Remove(user.Id);
        }
        
        // Constructor to initialize a game session with required details
        public GameSession(string gameId,User owner,string gameName,string image, uint requiredPlayersCount = 1)
        {
            this.gameId = gameId;
            this.gameName = gameName;
            this.image = image;
            this.owner = owner;
            this.users = new Dictionary<string, User>();
            this.requiredPlayersCount = requiredPlayersCount;
            this.id = Guid.NewGuid().ToString(); // Generate unique session ID
        }

    }
     
}