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

        // Constructor initializes database context and password hasher
        public MessageController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._passwordHasher = new PasswordHasher<User>();
        }

        // Get all messages for a specific chat
        [HttpGet("{chatId}/messages")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires access JWT token
        public async Task<IActionResult> GetMessages(string chatId)
        {
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return Unauthorized(new ApiSimpleResponse("user_not_found", "User was not found", "User was not found"));
            }

            // Get messages for the chat ordered by creation date
            var messages = await _applicationDBContext.Messages
                .Where(msg => msg.ChatId == chatId)
                .OrderBy(msg => msg.CreatedAt)
                .ToListAsync();

            // If no messages are found, return a NotFound response
            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found in this chat.");
            }

            return Ok(messages); // Return the list of messages
        }

        // Create a new message in a chat
        [HttpPost("{chatId}/new")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires access JWT token
        public async Task<IActionResult> CreateMessage(string chatId, [FromBody] Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Content)) // Ensure the message is not empty
            {
                return BadRequest(new ApiSimpleResponse("empty_message", "Empty message", "Empty message"));
            }

            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return Unauthorized(new ApiSimpleResponse("user_not_found", "User was not found", "User was not found"));
            }

            message.Id = Guid.NewGuid().ToString(); // Generate a unique message ID
            message.ChatId = chatId; // Set the chat ID
            message.UserId = userId; // Set the user ID (sender)
            message.CreatedAt = DateTime.UtcNow; // Set the message creation timestamp

            message.Chat = null; // Prevent navigation property issues
            message.Sender = null;

            _applicationDBContext.Messages.Add(message); // Add the message to the database
            await _applicationDBContext.SaveChangesAsync(); // Save the changes to the database

            return CreatedAtAction(nameof(GetMessages), new { chatId }, message); // Return the created message
        }

        // Delete a specific message from a chat
        [HttpDelete("{chatId}/{id}")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires access JWT token
        public async Task<IActionResult> DeleteMessage(string chatId, string id)
        {
            var userId = HttpContext.User.FindFirstValue("UserId");
            if (userId == null)
            {
                return Unauthorized(new ApiSimpleResponse("user_not_found", "User was not found", "User was not found"));
            }

            // Find the message by ID and check if it's associated with the current user
            var message = await _applicationDBContext.Messages
                .FirstOrDefaultAsync(msg => msg.Id == id && msg.ChatId == chatId && msg.UserId == userId);

            // If the message is not found, return a NotFound response
            if (message == null)
            {
                return NotFound(new ApiSimpleResponse("message_not_found", "Message was not found", "Message was not found"));
            }

            _applicationDBContext.Messages.Remove(message); // Remove the message
            await _applicationDBContext.SaveChangesAsync(); // Save changes to the database

            // Return success response
            return Ok(new ApiSimpleResponse("success", "Message was deleted", "Message was deleted"));
        }
    }
}
