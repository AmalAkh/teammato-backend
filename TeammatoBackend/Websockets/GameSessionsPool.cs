
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.WebSockets;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TeammatoBackend.WebSockets
{
    
    public class GameSessionPool
    {
        

        protected Dictionary<string, GameSession> gameSessions;

        public GameSessionPool()
        {
            gameSessions = new Dictionary<string, GameSession>();
        }

        public void Add(GameSession gameSession)
        {
            gameSessions.Add(gameSession.Id, gameSession);
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