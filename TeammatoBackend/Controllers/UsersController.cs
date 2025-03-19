
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
                issuer:"teammato-backend", 
                audience:"teammato-user"+user.Id, 
                expires:DateTime.UtcNow.Add(TimeSpan.FromDays(365))
                
            );
            jwtToken.Payload.Add("UserId", user.Id);
            return Ok(new JwtSecurityTokenHandler().WriteToken(jwtToken));
        }
    
        

    }
}