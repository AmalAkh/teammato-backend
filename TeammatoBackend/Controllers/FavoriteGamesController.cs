
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
using IGDB;
using IGDB.Models;
using Npgsql;
using dotenv.net;
using dotenv.net.Utilities;
namespace TeammatoBackend.Controllers
{
    [Route("api/favorite-games")]
    
    [ApiController]
    public class FavoriteGamesController : Controller
    {
        IGDBClient _iGDBClient;
        private readonly ApplicationDBContext _applicationDBContext;
        public FavoriteGamesController(ApplicationDBContext applicationDBContext)
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
        public async Task<IActionResult> NewFavoriteGame([FromBody]FavoriteGame game)
        {
            game.UserId = HttpContext.User.FindFirst("UserId").Value;
            try
            {
                _applicationDBContext.FavoriteGames.Add(game);
                await _applicationDBContext.SaveChangesAsync();
            
            }catch(DbUpdateException e) when((e.InnerException as PostgresException).SqlState == "23505")
            {
                return Conflict(new ApiSimpleResponse("duplicate_game", "Duplicate game"));
            }
            return Ok(new ApiSimpleResponse("success", "Game added","Game added"));
        }

        [HttpDelete("{gameId}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> DeleteFavoriteGame(string gameId)
        {
            var userId = HttpContext.User.FindFirst("UserId").Value;
            var targetGame = _applicationDBContext.FavoriteGames.FirstOrDefault((game)=>game.GameId == gameId && game.UserId == userId );
            if(targetGame == null)
            {
                return NotFound(new ApiSimpleResponse("game_not_found", "Game was not found","Game was not found"));
            }

            _applicationDBContext.FavoriteGames.Remove(targetGame);
            await _applicationDBContext.SaveChangesAsync();    
            return Ok(new ApiSimpleResponse("success", "Game was deleted","Game was deleted"));

        }

        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> ListGame()
        {
            var userId = HttpContext.User.FindFirst("UserId").Value;
            var games =  _applicationDBContext.FavoriteGames.Where(game=>game.UserId == userId).ToList();

            return Ok(games);
        }


    }
}