
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
    // Route for managing chat-related actions
    [Route("api/chats")]
    
    [ApiController]
    public class ChatsController : Controller
    {
        private readonly ApplicationDBContext _applicationDBContext;
        // Constructor to initialize the ApplicationDBContext
        public ChatsController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
         
        }

        // Endpoint to add a new chat (fetch user languages for chat creation)
        [HttpGet("new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> AddChat(string userId)
        {
            var targetUsr = _applicationDBContext.Users.FirstOrDefault(usr=>usr.Id == userId); // Find target user by userId
            if(targetUsr == null)
            {
                return NotFound(); // Return 404 if user not found
            }
            return Ok(targetUsr.Languages); // Return user's languages
        }

        [HttpDelete("{chatId}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> RemoveChat(string chatId)
        {
            var targetChat = _applicationDBContext.Chats.FirstOrDefault(chat=> chat.Id == chatId); // Find target user by userId
            if(targetChat == null)
            {
                return NotFound(); // Return 404 if user not found
            }
            _applicationDBContext.Chats.Remove(targetChat);
            await _applicationDBContext.SaveChangesAsync();
            return Ok(new ApiSimpleResponse("chat_removed", "Chat removed")); // Return user's languages
        }

        // Endpoint to get all chats for the currently authenticated user
        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> GetChats()
        {
            // Fetch user with chats and participants
            var userId = HttpContext.User.FindFirst("UserId")?.Value;

            var user = await _applicationDBContext.Users.FindAsync(userId);


            var chats = await _applicationDBContext.Chats
            .Where(chat => chat.Participants.Any(p => p.Id == userId)).Select((chat)=>new
            {   
                LastMessage = chat.Messages.OrderByDescending(msg=>msg.CreatedAt).FirstOrDefault(),
                chat.Id,
                chat.Name,
                chat.Image,
                Owner = new {chat.Owner.NickName, chat.Owner.Id, chat.Owner.Image},
                Participants = chat.Participants.Select(usr=>
                new {
                    usr.NickName,
                    usr.Id,
                    usr.Image
                }).ToList()
                
                
            }
            ).ToListAsync();

            if(user == null)
            {
                return NotFound("User not found"); // Return 404 if user not found
            }

            
            return Ok(chats);      // Return the user's chats with participants
        }
    }
}