
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
namespace TeammatoBackend.Controllers
{
    [Route("api/users")]
    
    [ApiController]
    public class UserController : Controller
    {
        private readonly ApplicationDBContext _applicationDBContext;
        public UserController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
        }
        [HttpPost("new")]
        
        public async Task<IActionResult> CreateNewUser([FromBody] User user)
        {
            user.Id = Guid.NewGuid().ToString();
            
            _applicationDBContext.Users.Add(user);
            
            _applicationDBContext.SaveChanges();
            var jwtToken = new JwtSecurityToken(
                issuer:JwtAuthOptions.Issuer, 
                audience:JwtAuthOptions.Audience, 
                claims:new List<Claim>(){new Claim("UserId", user.Id)},
                expires:DateTime.UtcNow.Add(TimeSpan.FromDays(365)),
                signingCredentials: new SigningCredentials(
                    JwtAuthOptions.GetAccessTokenSymmetricSecurityKey(), 
                    SecurityAlgorithms.HmacSha256
                )
                            
            );
            
            return Ok(new JwtSecurityTokenHandler().WriteToken(jwtToken));
        }
        [HttpGet("access_token")]
        public async Task<IActionResult> AccessToken()
        {

            var jwtToken = new JwtSecurityToken(
                issuer:JwtAuthOptions.Issuer, 
                audience:JwtAuthOptions.Audience, 
                claims:new List<Claim>(){new Claim("UserId", HttpContext.User.FindFirst("UserId")?.Value)},
                expires:DateTime.UtcNow.Add(TimeSpan.FromMinutes(1)),
                signingCredentials: new SigningCredentials(
                    JwtAuthOptions.GetAccessTokenSymmetricSecurityKey(), 
                    SecurityAlgorithms.HmacSha256
                )
                            
            );
            return Ok(jwtToken);
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