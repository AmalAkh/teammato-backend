
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TeammatoBackend.Abstractions;
using TeammatoBackend.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql.Util;
using Npgsql.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using TeammatoBackend.Utils;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TeammatoBackend.WebSockets;
using IGDB;
using IGDB.Models;
using dotenv.net;
using dotenv.net.Utilities;



namespace TeammatoBackend.Controllers
{
    // Configuration for initializing a new game session
    // Configuration for initializing a new game session
    public class GameSessionInitConfig
    {
        public string GameId{get;set;}
        public string ?Description {get;set;}
        public  uint PlayersCount {get;set;}
    }

    // Controller for managing game sessions
    [Route("api/gamesessions")]
    [ApiController]
    public class GameSessionsController : Controller
    {
        
        private readonly ApplicationDBContext _applicationDBContext;
        IGDBClient _iGDBClient;
        // Constructor initializes the database context and IGDB client
        public GameSessionsController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._iGDBClient = new IGDBClient(
                // Get IGDB client ID and secret from environment variables
                EnvReader.GetStringValue("IGDB_CLIENT_ID"),
                EnvReader.GetStringValue("IGDB_CLIENT_SECRET")
            );
        }

        // Endpoint to create a new game session
        [HttpPost("new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> NewGameSession([FromBody] GameSessionInitConfig config)
        {
            if(config.PlayersCount < 1)
            {
                return StatusCode(400); // Invalid player count
            }
            User owner = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value); // Get user from JWT token
            if(owner != null)
            {
                owner.Email = ""; // Hide sensitive data
                owner.Password = ""; // Hide sensitive data
                owner.Email = ""; // Hide sensitive data
                owner.Password = ""; // Hide sensitive data
            }
            
            // Get game cover and details from IGDB
            Cover cover = (await this._iGDBClient.QueryAsync<Cover>(IGDBClient.Endpoints.Covers, $"fields image_id, url; limit 1; where game={config.GameId};")).First();
            Game game = (await this._iGDBClient.QueryAsync<Game>(IGDBClient.Endpoints.Games, $"fields name; limit 1; where id={config.GameId};")).First();
            
            var cover_url = $"https:{cover.Url}".Replace("t_thumb","t_cover_big");
            var newGameSession = new GameSession(config.GameId, owner, game.Name, cover_url, config.PlayersCount);
            // Add new game session to the session pool
            lock(new object())
            {
                GameSessionsStorage.GameSessionPool.Add(newGameSession);
            }
            return Ok(newGameSession); // Return newly created game session
            return Ok(newGameSession); // Return newly created game session
        }

        // Endpoint for users to join an existing game session
        // Endpoint for users to join an existing game session
        [HttpPost("{sessionId}/join")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> JoinGameSession(string sessionId)
        {
            User user = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value); // Get user from JWT token
            lock(new object())
            {
                try
                {
                    GameSessionsStorage.GameSessionPool[sessionId].Join(user); // Add user to the game session
                }catch(KeyNotFoundException)
                {
                    // Game session not found
                    return NotFound(new ApiSimpleResponse("game_not_found", "Game was not found", "Game was not found"));
                }
            }
            
            // Create notification for new player
            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.NewPlayerJoined, user);
            // Notify all players in the session
            await WebSocketService.NotifyBySession(GameSessionsStorage.GameSessionPool[sessionId], notification);
            // Return success
            return Ok(new ApiSimpleResponse("success", "Your joined the game", "Your joined the game"));
        }

        // Endpoint for users to leave a game session
        [HttpPost("{sessionId}/leave")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> LeaveGameSession(string sessionId)
        {
            User user = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value); // Get user from JWT token
            lock(new object())
            {
                try
                {
                    if(!GameSessionsStorage.GameSessionPool[sessionId].Leave(user)) // Try to remove user from session
                    {
                        // User not found in session
                        return NotFound(new ApiSimpleResponse("user_not_found", "User did not participated", "User did not participated"));
                    }
                }catch(KeyNotFoundException)
                {
                    // Session not found
                    return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
                }
            }
            // Create notification for player leaving
            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.PlayerLeavedGameSession, user);
            // Notify all players in the session
            await WebSocketService.NotifyBySession(GameSessionsStorage.GameSessionPool[sessionId], notification);
            // Return success
            return Ok(new ApiSimpleResponse("success", "Your left the game", "Your left the game"));
        }

        // Endpoint to start the game session
        [HttpPost("{sessionId}/start")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> StartGameSession(string sessionId)
        {
            GameSession targetSession;
            lock(new object())
            {
                try
                {
                    targetSession = GameSessionsStorage.GameSessionPool[sessionId]; // Get target session
                    if(!GameSessionsStorage.GameSessionPool.Remove(sessionId)) // Remove session from pool
                    {
                        // Session not found
                        return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
                    }
                    
                }catch(KeyNotFoundException)
                {
                    // Session not found
                    return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
                }
            }
            // Create a chat for the game session
            Chat gameChat = new Chat();
            _applicationDBContext.Users.Attach(targetSession.Owner); // Attach owner to the chat
            gameChat.Participants.Add(targetSession.Owner);
            gameChat.Owner = targetSession.Owner;
            string chatName = "";
            foreach(var participant in targetSession.Users.Values)
            {
                _applicationDBContext.Users.Attach(participant); // Attach each participant to the chat
                gameChat.Participants.Add(participant);
                chatName+= participant.NickName;
            }
            gameChat.Id = Guid.NewGuid().ToString();        // Generate unique chat ID
            gameChat.Name = targetSession.GameName;                   // Set chat name
            gameChat.Image = targetSession.Image;
            _applicationDBContext.Chats.Add(gameChat);      // Add chat to the database
            await _applicationDBContext.SaveChangesAsync(); // Save changes to the database

            // Create notification for game start
            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.GameSessionStarted, new {ChatId = gameChat.Id});
            // Notify all players in the session
            await WebSocketService.NotifyBySession(targetSession, notification);
            // Return success
            return Ok(new ApiSimpleResponse("success", "Game started", "Game started"));
        }

        // Endpoint to get the list of users in a game session
        [HttpGet("{sessionId}/users")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> GameSessionUsers(string sessionId)
        {
            List<User> users;
            try
            {
                // Get all users in session except current user
                users = GameSessionsStorage.GameSessionPool[sessionId].Users.Values.Where((usr=>usr.Id != HttpContext.User.FindFirst("UserId").Value)).ToList();
            }catch(KeyNotFoundException)
            {
                // Session not found
                return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
            }
            return Ok(users); // Return list of users
        }
    }
}