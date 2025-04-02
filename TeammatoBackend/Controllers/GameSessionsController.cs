
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

            GameSessionsStorage.GameSessionPool.Add(newGameSession);

            return Ok(newGameSession);
        }
        
        
        

    }
}