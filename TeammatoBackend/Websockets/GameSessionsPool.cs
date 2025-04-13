
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
        
        // Dictionary to store game sessions by session ID
        protected Dictionary<string, GameSession> gameSessions;
        // Constructor to initialize the game session pool
        public GameSessionPool()
        {
            gameSessions = new Dictionary<string, GameSession>();
        }
        // Indexer to access game sessions by session ID
        public GameSession this[string sessionId]
        {
            get{return gameSessions[sessionId];}
        }
        // Method to add a game session to the pool
        public void Add(GameSession gameSession)
        {
            gameSessions.Add(gameSession.Id, gameSession);
        }
        // Method to remove a game session from the pool by session ID
        public bool Remove(string gameSessionId)
        {
            return gameSessions.Remove(gameSessionId);
        }
        // Method to add a user to a specific game session
        public void Join(string sessionId, User user)
        {
            gameSessions[sessionId].Join(user);
        } 
       // Method to get a game session by session ID
        public GameSession Get(string sessionId)
        {
            return gameSessions[sessionId];
        }
        // Method to get all game sessions in the pool
        public List<GameSession> GetGameSessions()
        {
            return gameSessions.Values.ToList();
        }
    }
     
}