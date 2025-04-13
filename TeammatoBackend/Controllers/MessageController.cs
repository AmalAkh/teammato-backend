using Microsoft.AspNetCore.Mvc;
using TeammatoBackend.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using TeammatoBackend.Abstractions;
using System.Security.Claims;

namespace TeammatoBackend.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ApplicationDBContext _applicationDBContext;
        public MessageController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._passwordHasher = new PasswordHasher<User>();
        }

        [HttpGet("{chatId}/messages")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> GetMessages(string chatId)
        {
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return Unauthorized(new ApiSimpleResponse("user_not_found", "User was not found", "User was not found"));
            }

            var messages = await _applicationDBContext.Messages
                .Where(msg => msg.ChatId == chatId)
                .OrderBy(msg => msg.CreatedAt)
                .ToListAsync();

            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found in this chat.");
            }

            return Ok(messages);
        }

        [HttpPost("{chatId}/new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> CreateMessage(string chatId, [FromBody] Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                return BadRequest(new ApiSimpleResponse("empty_message", "Empty message", "Empty message"));
            }

            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return Unauthorized(new ApiSimpleResponse("user_not_found", "User was not found", "User was not found"));
            }

            message.Id = Guid.NewGuid().ToString();
            message.ChatId = chatId;
            message.UserId = userId;
            message.CreatedAt = DateTime.UtcNow;

            message.Chat = null; // Prevent navigation property issues
            message.Sender = null;

            _applicationDBContext.Messages.Add(message);
            await _applicationDBContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMessages), new { chatId }, message);
        }

        [HttpDelete("{chatId}/{id}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> DeleteMessage(string chatId, string id)
        {
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return Unauthorized(new ApiSimpleResponse("user_not_found", "User was not found", "User was not found"));
            }

            var message = await _applicationDBContext.Messages
                .FirstOrDefaultAsync(msg => msg.Id == id && msg.ChatId == chatId && msg.UserId == userId);

            if (message == null)
            {
                return NotFound(new ApiSimpleResponse("message_not_found", "Message was not found", "Message was not found"));
            }

            _applicationDBContext.Messages.Remove(message);
            await _applicationDBContext.SaveChangesAsync();

            return Ok(new ApiSimpleResponse("success", "Message was deleted", "Message was deleted"));
        }
    }
}
