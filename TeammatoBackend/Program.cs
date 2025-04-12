using Microsoft.EntityFrameworkCore;
using TeammatoBackend;
using TeammatoBackend.Database;
using TeammatoBackend.Utils;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using System.Text;
using TeammatoBackend.Abstractions;
using TeammatoBackend.WebSockets;

// A new builder object is created and used to configure the application
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// Adds support for API documentation using OpenAPI 
builder.Services.AddOpenApi();
// Adds support for controllers that handle HTTP requests
builder.Services.AddControllers();
// Adds a service for working with a database via Entity Framework (specifically with the ApplicationDBContext database context)
builder.Services.AddDbContext<ApplicationDBContext>();
// Adds support for in-app authorization, which is necessary to work with JWT
builder.Services.AddAuthorization();

// Adds authentication using JWT
// access-jwt-token — is the primary access token that is used to authenticate users
// refresh-jwt-token — is an update token that is used to get a new access token without having to log in again
builder.Services.AddAuthentication("access-jwt-token").AddJwtBearer("refresh-jwt-token", (options)=>
{
    // Audience — for which application or service the token is intended (token consumer)
    options.Audience = JwtAuthOptions.Audience;
    // Authority — the token publisher, i.e. the service that issues the token
    options.Authority = JwtAuthOptions.Issuer;
    // specifies whether HTTPS will be used for metadata related to authentication and authorization
    options.RequireHttpsMetadata = false;
    // configuring token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
        {
            // specifies whether the publisher will be validated when validating the token
            ValidateIssuer = true,
            // string representing the publisher
            ValidIssuer = JwtAuthOptions.Issuer,
            // specifies whether the token consumer will be validated
            ValidateAudience = true,
            // string representing the token consumer
            ValidAudience = JwtAuthOptions.Audience,
            // whether the existence time will be validated
            ValidateLifetime = true,
            // security key installation
            IssuerSigningKey = JwtAuthOptions.GetRefreshTokenSymmetricSecurityKey(),
            // security key validation
            ValidateIssuerSigningKey = true,
         };
}).AddJwtBearer("access-jwt-token", (options)=>
{
    // Audience — for which application or service the token is intended (token consumer)
    options.Audience = JwtAuthOptions.Audience;
    // Authority — the token publisher, i.e. the service that issues the token
    options.Authority = JwtAuthOptions.Issuer;
    // specifies whether HTTPS will be used for metadata related to authentication and authorization
    options.RequireHttpsMetadata = false;
    // configuring token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
        {
            // specifies whether the publisher will be validated when validating the token
            ValidateIssuer = true,
            // string representing the publisher
            ValidIssuer = JwtAuthOptions.Issuer,
            // whether the consumer of the token will be validated
            ValidateAudience = true,
            // setting the token consumer
            ValidAudience = JwtAuthOptions.Audience,
            // whether the existence time will be validated
            ValidateLifetime = true,
            // setting the security key
            IssuerSigningKey = JwtAuthOptions.GetAccessTokenSymmetricSecurityKey(),
            // validate security key
            ValidateIssuerSigningKey = true,
         };
});

// Create an application instance based on the builder
var app = builder.Build();
app.UseMiddleware<CustomUnauthorizedMiddleware>();
// Enable authentication
app.UseAuthentication();
// Enable authorization
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions(){KeepAliveTimeout=TimeSpan.FromSeconds(4)});




// Adds endpoints for controller actions to the IEndpointRouteBuilder without specifying any routes.
app.MapControllers();

var websocketHandler = new WebSocketHandler();
app.Map("/ws", async (context)=>
{
    await WebSocketService.HandleConnection(context);
});

// Launching the application
app.Run();
