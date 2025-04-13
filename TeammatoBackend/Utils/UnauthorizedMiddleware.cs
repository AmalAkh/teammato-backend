using System.Text.Json;
using TeammatoBackend.Abstractions;
namespace TeammatoBackend.Utils
{
    public class CustomUnauthorizedMiddleware
    {
        private readonly RequestDelegate _next;

        // Constructor to initialize the middleware with the next delegate in the pipeline
        public CustomUnauthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Method to handle the request and check for unauthorized status
        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context); // Call the next middleware

            // If the response status is 401 Unauthorized and the response hasn't started
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json"; // Set response type to JSON
              
                // Serialize and write the custom error response
                var jsonResponse = JsonSerializer.Serialize(new ApiSimpleResponse("auth_failed", "Authorization failed"));
                await context.Response.WriteAsync(jsonResponse); // Send the response
            }
        }
    }
}