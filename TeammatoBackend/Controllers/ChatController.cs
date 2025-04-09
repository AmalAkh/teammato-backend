
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
    [Route("api/chats")]
    
    [ApiController]
    public class ChatsController : Controller
    {
        private readonly ApplicationDBContext _applicationDBContext;
        public ChatsController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
         
        }

        [HttpGet("new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> AddChat(string userId)
        {
            var targetUsr = _applicationDBContext.Users.FirstOrDefault(usr=>usr.Id == userId);
            if(targetUsr == null)
            {
                return NotFound();
            }
            return Ok(targetUsr.Languages);
        }

        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> GetChats()
        {
            
            var user = await _applicationDBContext.Users.Where(usr=>usr.Id == HttpContext.User.FindFirst("UserId").Value).Include((usr)=>usr.Chats).ThenInclude(chat=>chat.Participants).FirstOrDefaultAsync();
            if(user == null)
            {
                return NotFound("User not found");
            }
            user.Chats = user.Chats.Select((chat)=>
            {
                chat.Participants = chat.Participants.Select(usr=>
                {
                    usr.Password = "";
                    usr.Email = "";
                    return usr;
                }).ToList();
                return chat;
            }).ToList(); 
            return Ok(user.Chats);
        }
        

    }
}