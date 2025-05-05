
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
    // Route for managing favorite games
    [Route("api/favorite-games")]
    
    [ApiController]
    public class FavoriteGamesController : Controller
    {
        IGDBClient _iGDBClient;
        private readonly ApplicationDBContext _applicationDBContext;

        // Constructor initializes IGDB client and database context
        public FavoriteGamesController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._iGDBClient = new IGDBClient(
                // Get client ID and secret from environment variables
                EnvReader.GetStringValue("IGDB_CLIENT_ID"),
                EnvReader.GetStringValue("IGDB_CLIENT_SECRET")
                
                );
        }
        
        // Endpoint to add a new favorite game
        [HttpPost("new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> NewFavoriteGame([FromBody]FavoriteGame game)
        {
            game.UserId = HttpContext.User.FindFirst("UserId").Value; // Assign user ID from JWT
            try
            {
                _applicationDBContext.FavoriteGames.Add(game);  // Add game to the database
                await _applicationDBContext.SaveChangesAsync(); // Save changes to the database
            
            }catch(DbUpdateException e) when((e.InnerException as PostgresException).SqlState == "23505")
            {
                return Conflict(new ApiSimpleResponse("duplicate_game", "Duplicate game")); // Handle duplicates
            }
            return Ok(new ApiSimpleResponse("success", "Game added","Game added")); // Return success
        }

        // Endpoint to delete a favorite game
        [HttpDelete("{gameId}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> DeleteFavoriteGame(string gameId)
        {
            var userId = HttpContext.User.FindFirst("UserId").Value; // Get user ID from JWT
            var targetGame = _applicationDBContext.FavoriteGames.FirstOrDefault((game)=>game.GameId == gameId && game.UserId == userId); // Find game for the user
            if(targetGame == null)
            {
                return NotFound(new ApiSimpleResponse("game_not_found", "Game was not found","Game was not found")); // Return 404 if game not found
            }

            _applicationDBContext.FavoriteGames.Remove(targetGame); // Remove game from the database
            await _applicationDBContext.SaveChangesAsync();         // Save changes to the database
            return Ok(new ApiSimpleResponse("success", "Game was deleted","Game was deleted")); // Return success

        }

        // Endpoint to list all favorite games for the current user
        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> ListGame()
        {
            var userId = HttpContext.User.FindFirst("UserId").Value; // Get user ID from JWT
            var games =  _applicationDBContext.FavoriteGames.Where(game=>game.UserId == userId).ToList(); // Get games for the user

            return Ok(games); // Return list of favorite games
        }
        public record GameSearchRequest(string Name);
        [HttpPost("available-list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> ListAvailableGames([FromBody] GameSearchRequest request)
        {
            
            string query = $"search \"{request.Name}\";fields name, id, cover;";
            var games = (await this._iGDBClient.QueryAsync<Game>(IGDBClient.Endpoints.Games, query )).ToList<Game>();
            var convertedGames = games.Select((game)=>new FavoriteGame(){ Name=game.Name, GameId=game.Id.ToString(), Image=game.Cover.Id.ToString()});
            return Ok(convertedGames);
        }


    }
}