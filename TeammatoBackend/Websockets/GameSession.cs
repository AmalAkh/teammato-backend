
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
        protected string description;
        public string Description
        {
            get
            {
                return description;
            }
        }
        protected string gameName;
        public string GameName
        {
            get
            {
                return gameName;
            }
        }
        protected string imageId;
        public string ImageId
        {
            get
            {
                return imageId;
            }
        }
        protected string id;
        public string Id
        {
            get
            {
                return id;
            }
        }
        public string GameId
        {
            get
            {
                return gameId;
            }
        }

        protected string gameId;


        public uint RequiredPlayersCount
        {
            get
            {
                return requiredPlayersCount;
            }
        }

        protected uint requiredPlayersCount;

        protected User owner;
        public User Owner
        {
            get
            {
                return owner;
            }
        }
        protected List<User> users;

        public List<User> Users
        {
            get{return users;}
        }

        public void Join(User user)
        {
            users.Add(user);
        }

        public GameSession(string gameId,User owner,string gameName,string imageId, uint requiredPlayersCount = 1)
        {
            this.gameId = gameId;
            this.gameName = gameName;
            this.imageId = imageId;
            this.owner = owner;
            this.users = new List<User>();
            this.requiredPlayersCount = requiredPlayersCount;
            this.id = Guid.NewGuid().ToString();
        }

    }
     
}