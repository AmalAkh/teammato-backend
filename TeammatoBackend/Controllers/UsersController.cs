
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
    [Route("api/users")]
    
    [ApiController]
    public class UserController : Controller
    {
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ApplicationDBContext _applicationDBContext;
        public UserController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._passwordHasher = new PasswordHasher<User>();
        }
        [HttpPost("new")]
        
        public async Task<IActionResult> CreateNewUser([FromBody] User user)
        {
            user.Id = Guid.NewGuid().ToString();
            user.Password = _passwordHasher.HashPassword(user, user.Password);
            _applicationDBContext.Users.Add(user);
            try
            {
                _applicationDBContext.SaveChanges();
            }catch(DbUpdateException e)
            {
                return BadRequest();
            }
           
            var jwtToken =  JwtTokenGenerator.GenerateRefreshToken(new List<Claim>(){ new Claim("UserId", user.Id) });
            return Ok(new JwtSecurityTokenHandler().WriteToken(jwtToken));
        }
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInData signInData)
        {
            var targetUser = await _applicationDBContext.Users.FirstOrDefaultAsync<User>((usr)=>usr.Email == signInData.Login);
            if(targetUser == null)
            {
                return NotFound();
            } 
            if(_passwordHasher.VerifyHashedPassword(targetUser,targetUser.Password, signInData.Password) == PasswordVerificationResult.Success)
            {
                string token = new JwtSecurityTokenHandler().WriteToken(JwtTokenGenerator.GenerateRefreshToken(new List<Claim>(){ new Claim("UserId", targetUser.Id) }));
                return Ok(token);

            }
            return Unauthorized();
        }
        [HttpGet("access-token")]
        [Authorize(AuthenticationSchemes = "refresh-jwt-token")]

        public async Task<IActionResult> AccessToken()
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            string token = new JwtSecurityTokenHandler().WriteToken(JwtTokenGenerator.GenerateAccessToken(new List<Claim>(){ new Claim("UserId", HttpContext.User.FindFirstValue("UserId")) }));
            return Ok(token);
        }
        [HttpGet("test")]
        [Authorize(AuthenticationSchemes = "refresh-jwt-token")]
        public async Task<IActionResult> Test()
        {
            
            return Ok(HttpContext.User.FindFirst("UserId")?.Value);
        }
        [HttpGet("test2")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> Test2()
        {
            
            return Ok(HttpContext.User.FindFirst("UserId")?.Value);
        }


       
        

    }
}