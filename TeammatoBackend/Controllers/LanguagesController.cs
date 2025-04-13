
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
using System.Net;


using System.Text.Json;
using Npgsql;
namespace TeammatoBackend.Controllers
{
    [Route("api/languages")]
    
    [ApiController]
    public class LanguageController : Controller
    {
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ApplicationDBContext _applicationDBContext;
        // Constructor to initialize DB context and password hasher
        public LanguageController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._passwordHasher = new PasswordHasher<User>();
        }
    
        // Get all languages for a specific user
        [HttpGet("{userId}/languages")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> GetLanguages(string userId)
        {
            // Get user by ID
            var targetUsr = _applicationDBContext.Users.FirstOrDefault(usr=>usr.Id == userId);
            if(targetUsr == null)
            {
                // User not found
                return NotFound(new ApiSimpleResponse("user_not_found", "User was not found"));
            }
            // Return user's languages
            return Ok(targetUsr.Languages);
        }

        // Add a new language to the current user
        [HttpPost("new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> AddLanguage([FromBody] Language language)
        {
            language.UserId = HttpContext.User.FindFirst("UserId")?.Value; // Set current user ID
            try
            {
                _applicationDBContext.Languages.Add(language); // Add language to DB
                _applicationDBContext.SaveChanges(); // Save changes
            }
            catch(DbUpdateException e) when((e.InnerException as PostgresException).SqlState == "23505") // Handle duplicate entry
            {
                // Return conflict on duplicate language
                return Conflict(new ApiSimpleResponse("duplicate_language", "Duplicate language"));
            }
            // Return success
            return Ok(new ApiSimpleResponse("success", "Language was added", "Duplicate language"));
        }

        // Get all languages for the current user
        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> GetLanguages()
        {
            // Return languages of the current user
            return Ok(_applicationDBContext.Languages.Where((lang)=>lang.UserId == HttpContext.User.FindFirstValue("UserId")));
        }

        // Delete a language for the current user
        [HttpDelete("{langCode}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token
        public async Task<IActionResult> DeleteLanguage(string langCode)
        {
            var language = _applicationDBContext.Languages
            // Find language by code and user ID
            .FirstOrDefault(lang => lang.UserId == HttpContext.User.FindFirstValue("UserId") && lang.ISOName == langCode);

            if (language == null)
            {
                // Language not found
                return NotFound(new ApiSimpleResponse("language_not_found", "Language was not found"));
            }
            _applicationDBContext.Languages.Remove(language); // Remove language from DB
            int changes = await _applicationDBContext.SaveChangesAsync(); // Save changes

            if (changes > 0)
            {
                // Return success if changes were saved
                return Ok(new ApiSimpleResponse("success", "Language was deleted"));
            }
            // Return error if no changes were made
            return StatusCode(500);
        }

        // Test endpoint (currently not requiring authorization)
        [HttpGet("test")]
        //[Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> Test()
        {
            // Return languages of first user found
            return Ok(_applicationDBContext.Users.Include(user=>user.Languages).FirstOrDefault().Languages);
        } 
        

    }
}