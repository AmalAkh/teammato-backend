
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
    
    public class GameSessionPool
    {
        

        protected Dictionary<string, GameSession> gameSessions;

        public GameSessionPool()
        {
            gameSessions = new Dictionary<string, GameSession>();
        }
        public GameSession this[string sessionId]
        {
            get{return gameSessions[sessionId];}
        
        }
        public void Add(GameSession gameSession)
        {
            gameSessions.Add(gameSession.Id, gameSession);
        }
        public void Join(string sessionId, User user)
        {
            gameSessions[sessionId].Join(user);
        }   

        public GameSession Get(string sessionId)
        {
            return gameSessions[sessionId];
        }
        public List<GameSession> GetGameSessions()
        {
            return gameSessions.Values.ToList();
        }



    }
     
}