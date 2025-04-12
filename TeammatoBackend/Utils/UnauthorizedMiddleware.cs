using System.Text.Json;
using TeammatoBackend.Abstractions;
namespace TeammatoBackend.Utils
{
    public class CustomUnauthorizedMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomUnauthorizedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                context.Response.ContentType = "application/json";
              
                var jsonResponse = JsonSerializer.Serialize(new ApiSimpleResponse("auth_failed", "Authorization failed"));
                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}