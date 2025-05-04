
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
using System.Text.RegularExpressions;
namespace TeammatoBackend.Controllers
{
    // Controller for managing users
    [Route("api/users")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ApplicationDBContext _applicationDBContext;

        // Constructor to initialize DB context and password hasher
        public UserController(ApplicationDBContext applicationDBContext)
        {
            this._applicationDBContext = applicationDBContext;
            this._passwordHasher = new PasswordHasher<User>();
        }

        // Create a new user and return a JWT token
        [HttpPost("new")]
        public async Task<IActionResult> CreateNewUser([FromBody] User user)
        {
            user.Id = Guid.NewGuid().ToString(); // Generate unique user ID
            user.Password = _passwordHasher.HashPassword(user, user.Password); // Hash the user's password
            _applicationDBContext.Users.Add(user); // Add user to the database
            try
            {
                _applicationDBContext.SaveChanges(); // Save changes to the database
            }
            catch(DbUpdateException e) // Handle duplicate user scenario
            {
                return BadRequest(new ApiSimpleResponse("duplicate_user", "Duplicate user", "Duplicate user"));
            }
           
            // Generate a JWT token for the user
            var jwtToken =  JwtTokenGenerator.GenerateRefreshToken(new List<Claim>(){ new Claim("UserId", user.Id) });
            return Ok(new JwtSecurityTokenHandler().WriteToken(jwtToken)); // Return JWT token
        }

        // User sign-in with login and password
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInData signInData)
        {
            var targetUser = await _applicationDBContext.Users.FirstOrDefaultAsync<User>((usr)=>usr.Email == signInData.Login);
            if(targetUser == null) // If user is not found
            {
                return NotFound(new ApiSimpleResponse("user_not_found", "User was not found", "User was not found"));
            } 
            // Verify user's password
            if(_passwordHasher.VerifyHashedPassword(targetUser,targetUser.Password, signInData.Password) == PasswordVerificationResult.Success)
            {
                // Generate a JWT token for successful sign-in
                string token = new JwtSecurityTokenHandler().WriteToken(JwtTokenGenerator.GenerateRefreshToken(new List<Claim>(){ new Claim("UserId", targetUser.Id) }));
                return Ok(token); // Return token
            }
            // Invalid credentials
            return Unauthorized(new ApiSimpleResponse("auth_failed", "Authorization failed","Authorization failed"));
        }

        // Upload a profile image for the user
        [HttpPut("upload-image")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires access JWT token
        public async Task<IActionResult> UploadImage()
        {
            var allowedExtensions = new string[]{"jpeg", "jpg", "png"}; // Allowed image formats
            if(!HttpContext.Request.HasFormContentType)
            {
                return BadRequest(); // Bad request if no content type
            }
            var image = HttpContext.Request.Form.Files[0]; // Get the uploaded image
            if(image.Length <= 0 && image.Length > 10485760) // Validate image size
            {   
                return BadRequest(); // Bad request if invalid size
            }
            var extension = image.ContentType.Split("/")[1]; // Get file extension
            if(!allowedExtensions.Contains(extension)) // Validate extension
            {
                return BadRequest(new ApiSimpleResponse("ext_not_allowed", "Extension is not allowed","Extension is not allowed"));
            }

            var newFilename = Guid.NewGuid().ToString() + "."+extension; // Generate new filename

            using(var stream = System.IO.File.Create(Path.Join("UserContent", newFilename)))
            {
                await image.CopyToAsync(stream); // Save the image file
            }
            var userId = HttpContext.User.FindFirst("UserId")?.Value; // Get the current user's ID
            var user = _applicationDBContext.Users.FirstOrDefault(usr=>usr.Id == userId); // Find the user
            user.Image = newFilename; // Set the user's image filename
            await _applicationDBContext.SaveChangesAsync(); // Save changes
            return Ok(newFilename); // Return the filename of the uploaded image
        }
        [HttpGet("info")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            var user = _applicationDBContext.Users.Where((usr)=>usr.Id == userId).Select(usr => new {
                usr.Id,
                usr.NickName,
                
                
                usr.Image
                // include all fields you want, EXCLUDE Email and Password
            }).FirstOrDefault();
            return Ok(user);
        }

        // Define a record to transmit data about a new nickname
        public record NickNameUpdateData(string newNickname);
        // Update the user's nickname
        [HttpPut("update-nickname")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires access JWT token
        public async Task<IActionResult> UpdateNickname([FromBody] NickNameUpdateData nickNameUpdateData)
        {
            // Get the current user's ID from the token
            var userId = HttpContext.User.FindFirst("UserId")?.Value;
            
            // Find the user by ID
            var user = await _applicationDBContext.Users.FirstOrDefaultAsync(usr => usr.Id == userId);
            
            if (user == null) // If user is not found
            {
                return NotFound(new ApiSimpleResponse("user_not_found", "User not found", "User not found"));
            }
            
            // Update the nickname
            user.NickName = nickNameUpdateData.newNickname;
            // Save the changes to the database
            await _applicationDBContext.SaveChangesAsync();
            // Return a successful response with a message
            return Ok(new ApiSimpleResponse("success", "Nickname updated successfully", "Nickname updated successfully"));
        }


        // Generate a new access token based on refresh token
        [HttpGet("access-token")]
        [Authorize(AuthenticationSchemes = "refresh-jwt-token")] // Requires refresh JWT token

        public async Task<IActionResult> AccessToken()
        {
            var userId = HttpContext.User.FindFirst("UserId")?.Value; // Get user ID from token
            string token = new JwtSecurityTokenHandler().WriteToken(JwtTokenGenerator.GenerateAccessToken(new List<Claim>(){ new Claim("UserId", HttpContext.User.FindFirstValue("UserId")) }));
            return Ok(token); // Return the new access token
        }

        // Test endpoint for verifying access token
        [HttpGet("test")]
        [Authorize(AuthenticationSchemes = "refresh-jwt-token")] // Requires refresh JWT token
        public async Task<IActionResult> Test()
        {
            return Ok(HttpContext.User.FindFirst("UserId")?.Value); // Return the user ID from the token
        }

        // Another test endpoint for verifying access token
        [HttpGet("test2")]
        [Authorize(AuthenticationSchemes = "access-jwt-token")] // Requires access JWT token
        public async Task<IActionResult> Test2()
        {
            return Ok(HttpContext.User.FindFirst("UserId")?.Value); // Return the user ID from the token
        }
    }
}