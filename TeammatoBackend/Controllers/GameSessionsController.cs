
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
using Microsoft.AspNetCore.Authentication.JwtBearer;



namespace TeammatoBackend.Controllers
{
    // Configuration for initializing a new game session
   
    public class GameSessionInitConfig
    {
        public string GameId{get;set;}
        public string ?Description {get;set;}
        public  uint PlayersCount {get;set;}
        public double Duration{get;set;}
      
        public List<string> Languages {get;set;}

    }
    public class GameSessionSearchConfig
    {
        public bool Nearest { get; set; }
        public string ?Description {get;set;}
        public  uint PlayersCount {get;set;}
        public List<string> Languages {get;set;}
        public List<string> GameIds {get;set;}
        public double DurationFrom{get;set;}
        public double DurationTo{get;set;}
        public double Latitude{get;set;}
        public double Longitude{get;set;}
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
            
            
            // Get game cover and details from IGDB
            Cover cover = (await this._iGDBClient.QueryAsync<Cover>(IGDBClient.Endpoints.Covers, $"fields image_id, url; limit 1; where game={config.GameId};")).First();
            Game game = (await this._iGDBClient.QueryAsync<Game>(IGDBClient.Endpoints.Games, $"fields name; limit 1; where id={config.GameId};")).First();
            
            var cover_url = $"https:{cover.Url}".Replace("t_thumb","t_cover_big");
            var newGameSession = new GameSession()
            {
                GameId = config.GameId, Owner=owner, GameName = game.Name, Image = cover_url, RequiredPlayersCount = config.PlayersCount,
                Id = Guid.NewGuid().ToString(),
                Duration = config.Duration,
                Langs = string.Join(",", config.Languages)
            
            };
            newGameSession.Participants.Add(owner);
            // Add new game session to the session pool
            
            await _applicationDBContext.GameSessions.AddAsync(newGameSession);
            await _applicationDBContext.SaveChangesAsync();

            var response = new GameSession()
            {
                GameId = config.GameId,
                Owner = new User { Id = owner.Id, NickName = owner.NickName}, // Don't include Email or Password
                GameName = game.Name,
                Image = cover_url,
                RequiredPlayersCount = config.PlayersCount,
                Id = newGameSession.Id,
                Participants = new List<User> { owner }
            };

            return Ok(response); // Return newly created game session
           
        }

        // Endpoint for users to join an existing game session
        // Endpoint for users to join an existing game session
        [HttpPost("{sessionId}/join")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> JoinGameSession(string sessionId)
        {
            User user = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value); // Get user from JWT token
            
            
            // Create notification for new player
            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.NewPlayerJoined, 
            new User(){NickName = user.NickName, Id= user.Id, Image = user.Image});
            // Notify all players in the session
            GameSession targetSession = await _applicationDBContext.GameSessions
            .Where((session)=>session.Id == sessionId)
            .Include((session)=>session.Owner)
            .Include((session)=>session.Participants)
            .FirstAsync();
            if(targetSession == null)
            {
                return NotFound(new ApiSimpleResponse("game_not_found", "Game was not found", "Game was not found"));

            }
            targetSession.Participants.Add(user);
            await _applicationDBContext.SaveChangesAsync();
            await WebSocketService.NotifyBySession(targetSession, notification);
            // Return success
            
            return Ok(new ApiSimpleResponse("success", "Your joined the game", "Your joined the game"));
        }

        // Endpoint for users to leave a game session
        [HttpPost("{sessionId}/leave")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> LeaveGameSession(string sessionId)
        {
            User user = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value); // Get user from JWT token
            GameSession targetSession = await _applicationDBContext.GameSessions
            .Where((session)=>session.Id == sessionId)
            .Include((session)=>session.Owner)
            .Include((session)=>session.Participants)
            .FirstAsync();
             // Get target session
            if(targetSession == null) // Remove session from pool
            {
                // Session not found
                return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
            }
            targetSession.Participants.Remove(user);
            await _applicationDBContext.SaveChangesAsync();
            // Create notification for player leaving
            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.PlayerLeavedGameSession, new {user.Id, user.NickName});
            // Notify all players in the session
            await WebSocketService.NotifyBySession(targetSession, notification);
            // Return success
            return Ok(new ApiSimpleResponse("success", "Your left the game", "Your left the game"));
        }
        [HttpDelete("{sessionId}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> DeleteGameSession(string sessionId)
        {
            User user = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value);
            GameSession targetSession = await _applicationDBContext.GameSessions
            .Include((session)=>session.Owner)
            .Include((session)=>session.Participants)
            .Where((session)=>session.Id == sessionId)
            .FirstAsync();
            if(targetSession == null) 
            {
                // Session not found
                return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
            }
            if(targetSession.Owner.Id != user.Id)
            {
                return Forbid();
            }
            _applicationDBContext.GameSessions.Remove(targetSession);

            
            
            await _applicationDBContext.SaveChangesAsync();
            // Create notification for player leaving
            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.GameSessionCancelled, new object());
            // Notify all players in the session
            await WebSocketService.NotifyBySession(targetSession, notification);
            // Return success
            return Ok(new ApiSimpleResponse("success", "Your left the game", "Your left the game"));
        }

        // Endpoint to start the game session
        [HttpPost("{sessionId}/start")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> StartGameSession(string sessionId)
        {
            GameSession targetSession ;
            try
            {
                targetSession = await _applicationDBContext.GameSessions
                .Where((session)=>session.Id == sessionId)
                .Include((session)=>session.Owner)
                .Include((session)=>session.Participants)
                .FirstAsync();
            }catch(InvalidOperationException e)
            {
                return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
            }
             // Get target session
            if(targetSession == null) // Remove session from pool
            {
                // Session not found
                return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
            }
                    
                
            
            // Create a chat for the game session
            Chat gameChat = new Chat();
            _applicationDBContext.Users.Attach(targetSession.Owner); // Attach owner to the chat
            gameChat.Participants.Add(targetSession.Owner);
            gameChat.Owner = targetSession.Owner;
            string chatName = "";
            foreach(var participant in targetSession.Participants)
            {
                if(participant.Id == gameChat.Owner.Id)
                {
                    continue;
                }
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
            _applicationDBContext.Remove(targetSession);
            await _applicationDBContext.SaveChangesAsync();
            // Return success
            return Ok(gameChat.Id);
        }

        // Endpoint to get the list of users in a game session
        [HttpGet("{sessionId}/users")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> GameSessionUsers(string sessionId)
        {
           
                // Get all users in session except current user
            GameSession targetSession = await _applicationDBContext.GameSessions
            .Where((session)=>session.Id == sessionId)
            .Include((session)=>session.Owner)
            .Include((session)=>session.Participants)
            .FirstAsync();

            if(targetSession == null)
            {
                return NotFound(new ApiSimpleResponse("game_session_not_found", "Game session was not found", "Game session was not found"));
            }
                // Session not found
                
            
            return Ok(targetSession.Participants.Select((participant)=>
            new {
                participant.NickName,
                participant.Id,
                participant.Image
            })); // Return list of users*/
         
        }
        [HttpPost("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> GameSessionsList([FromBody]GameSessionSearchConfig gameSessionSearchConfig)
        {
           
                // Get all users in session except current user
            List<GameSession> targetSessions = await _applicationDBContext.GameSessions
            .Where(
                (session)=>session.RequiredPlayersCount == gameSessionSearchConfig.PlayersCount
                && gameSessionSearchConfig.GameIds.Contains(session.GameId)
                && (session.Duration >= gameSessionSearchConfig.DurationFrom)
                && (session.Duration <= gameSessionSearchConfig.DurationTo)
                
            
                ).Include((session)=>session.Participants)
                .Include((session)=>session.Owner).ToListAsync();

            var result = targetSessions.OrderBy((session)=>
            {
                double score = 0;

                foreach(var lang in gameSessionSearchConfig.Languages)
                {
                    if(session.Langs.Contains(lang))
                    {
                        score+=1;
                    }
                }
                if(gameSessionSearchConfig.Nearest)
                {
                    score+=Math.Abs(session.Latitude - gameSessionSearchConfig.Latitude);
                }
                if(gameSessionSearchConfig.Nearest)
                {
                    score+=Math.Abs(session.Longitude - gameSessionSearchConfig.Longitude);
                }


                

                return score;
            }).Select(session => 
                new
                {
                    session.Description,
                    session.GameId,
                    session.GameName,
                    session.Image,
                    session.Id,
                    Duration = session.Duration,
                    Participants = session.Participants.Select(participant => new { participant.Id }).ToList(),
                    session.RequiredPlayersCount,
                    Owner=new{session.Owner.NickName, session.Owner.Id}
                    
                }
            ).ToList();
            

           
                // Session not found
                
            
            return Ok(result); // Return list of users*/
         
        }
    }
}