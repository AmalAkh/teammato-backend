
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
    public class GameSessionInitConfig
    {
        public string GameId{get;set;}
        public string ?Description {get;set;}
        public  uint PlayersCount {get;set;}

    }
    [Route("api/gamesessions")]
    
    [ApiController]
    public class GameSessionsController : Controller
    {
        
        private readonly ApplicationDBContext _applicationDBContext;
        IGDBClient _iGDBClient;
        public GameSessionsController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._iGDBClient = new IGDBClient(
            // Found in Twitch Developer portal for your app
            EnvReader.GetStringValue("IGDB_CLIENT_ID"),
            EnvReader.GetStringValue("IGDB_CLIENT_SECRET")
            
            );
        }

        [HttpPost("new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> NewGameSession([FromBody] GameSessionInitConfig config)
        {
            if(config.PlayersCount < 1)
            {
                return StatusCode(400);
            }
            User owner = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value);
            if(owner != null)
            {
                owner.Email = "";
                owner.Password = "";
            }
            
            Cover cover = (await this._iGDBClient.QueryAsync<Cover>(IGDBClient.Endpoints.Covers, $"fields image_id; limit 1; where game={config.GameId};")).First();
            Game game = (await this._iGDBClient.QueryAsync<Game>(IGDBClient.Endpoints.Games, $"fields name; limit 1; where id={config.GameId};")).First();
            
            var newGameSession = new GameSession(config.GameId, owner, game.Name, cover.ImageId, config.PlayersCount);
            lock(new object())
            {
                GameSessionsStorage.GameSessionPool.Add(newGameSession);
            }
            return Ok(newGameSession);
        }

        [HttpPost("{sessionId}/join")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> JoinGameSession(string sessionId)
        {
            User user = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value);
            lock(new object())
            {
                try
                {
                    GameSessionsStorage.GameSessionPool[sessionId].Join(user);
                }catch(KeyNotFoundException)
                {
                    return NotFound();
                }
            }
            

            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.NewPlayerJoined, user);
            await WebSocketService.NotifyBySession(GameSessionsStorage.GameSessionPool[sessionId], notification);
            return Ok();
        }

        [HttpPost("{sessionId}/leave")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> LeaveGameSession(string sessionId)
        {
            User user = await _applicationDBContext.Users.FindAsync(HttpContext.User.FindFirst("UserId")?.Value);
            lock(new object())
            {
                try
                {
                    if(!GameSessionsStorage.GameSessionPool[sessionId].Leave(user))
                    {
                        return NotFound(404);
                    }
                }catch(KeyNotFoundException)
                {
                    return NotFound(404);
                }
            }

            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.PlayerLeavedGameSession, user);
            await WebSocketService.NotifyBySession(GameSessionsStorage.GameSessionPool[sessionId], notification);
            return Ok();
        }

        [HttpPost("{sessionId}/start")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> StartGameSession(string sessionId)
        {
            
            GameSession targetSession;
            lock(new object())
            {
                try
                {
                    targetSession = GameSessionsStorage.GameSessionPool[sessionId];
                    if(!GameSessionsStorage.GameSessionPool.Remove(sessionId))
                    {
                        return NotFound();
                    }
                    
                }catch(KeyNotFoundException)
                {
                    return NotFound();
                }
                
                
            }

            Chat gameChat = new Chat();
            _applicationDBContext.Users.Attach(targetSession.Owner);
            gameChat.Participants.Add(targetSession.Owner);
            string chatName = "";
            foreach(var participant in targetSession.Users.Values)
            {
                _applicationDBContext.Users.Attach(participant);
                gameChat.Participants.Add(participant);
                chatName+= participant.NickName;
            }
            gameChat.Id = Guid.NewGuid().ToString();
            gameChat.Name = "Test chat";
            _applicationDBContext.Chats.Add(gameChat);
            await _applicationDBContext.SaveChangesAsync();

            var notification = WebSocketNotificationFactory.CreateNotification(WebSocketNotificationType.GameSessionStarted, new {ChatId = gameChat.Id});
            await WebSocketService.NotifyBySession(targetSession, notification);

            return Ok();
        }

        [HttpGet("{sessionId}/users")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> GameSessionUsers(string sessionId)
        {
            List<User> users;
            try
            {
                users = GameSessionsStorage.GameSessionPool[sessionId].Users.Values.Where((usr=>usr.Id != HttpContext.User.FindFirst("UserId").Value)).ToList();
            }catch(KeyNotFoundException)
            {
                return NotFound();
            }
            return Ok(users);
        }


        
        
        

    }
}