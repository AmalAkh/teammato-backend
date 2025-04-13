
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

        // Endpoint to get all chats for the currently authenticated user
        [HttpGet("list")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires JWT token for authentication
        public async Task<IActionResult> GetChats()
        {
            // Fetch user with chats and participants
            var user = await _applicationDBContext.Users
                .Where(usr=>usr.Id == HttpContext.User.FindFirst("UserId").Value) // Get user by ID from JWT
                .Include((usr)=>usr.Chats)
                .ThenInclude(chat=>chat.Participants)
                .FirstOrDefaultAsync();
            if(user == null)
            {
                return NotFound("User not found"); // Return 404 if user not found
            }

            // Remove sensitive information from participant users
            user.Chats = user.Chats.Select((chat)=>
            {
                chat.Participants = chat.Participants.Select(usr=>
                {
                    usr.Password = "";  // Clear password
                    usr.Email = "";     // Clear email
                    return usr;
                }).ToList();
                return chat;
            }).ToList(); 
            return Ok(user.Chats);      // Return the user's chats with participants
        }
    }
}