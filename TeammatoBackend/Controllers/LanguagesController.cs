
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
namespace TeammatoBackend.Controllers
{
    [Route("api/languages")]
    
    [ApiController]
    public class LanguageController : Controller
    {
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ApplicationDBContext _applicationDBContext;
        public LanguageController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._passwordHasher = new PasswordHasher<User>();
        }
        
        [HttpGet("{userId}/languages")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> GetLanguages(string userId)
        {
            var targetUsr = _applicationDBContext.Users.FirstOrDefault(usr=>usr.Id == userId);
            if(targetUsr == null)
            {
                return NotFound();
            }
            return Ok(targetUsr.Languages);
        }

        [HttpPost("new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> AddLanguage([FromBody] Language language)
        {
            language.UserId = HttpContext.User.FindFirst("UserId")?.Value;
            _applicationDBContext.Languages.Add(language);
            _applicationDBContext.SaveChanges();
            return Ok();
        }
        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> GetLanguages()
        {

            return Ok(_applicationDBContext.Languages.Where((lang)=>lang.UserId == HttpContext.User.FindFirstValue("UserId")));
        }

        [HttpDelete("{langCode}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> DeleteLanguage(string langCode)
        {
            var language = _applicationDBContext.Languages
            .FirstOrDefault(lang => lang.UserId == HttpContext.User.FindFirstValue("UserId") && lang.ISOName == langCode);

            if (language == null)
            {
                return NotFound();
            }
            _applicationDBContext.Languages.Remove(language);
            int changes = await _applicationDBContext.SaveChangesAsync();

            if (changes > 0)
            {
                return Ok(new { message = "Language deleted successfully" });
            }
            return StatusCode(500);

        }
        [HttpGet("test")]
        //[Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> Test()
        {

            
            return Ok(_applicationDBContext.Users.Include(user=>user.Languages).FirstOrDefault().Languages);
        } 
        

    }
}