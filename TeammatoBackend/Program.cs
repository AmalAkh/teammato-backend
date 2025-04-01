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

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDBContext>();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("access-jwt-token").AddJwtBearer("refresh-jwt-token", (options)=>
{
    options.Audience = JwtAuthOptions.Audience;
    options.Authority = JwtAuthOptions.Issuer;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
        {
            
            // указывает, будет ли валидироваться издатель при валидации токена
            ValidateIssuer = true,
            // строка, представляющая издателя
            ValidIssuer = JwtAuthOptions.Issuer,
            // будет ли валидироваться потребитель токена
            ValidateAudience = true,
            // установка потребителя токена
            ValidAudience = JwtAuthOptions.Audience,
            // будет ли валидироваться время существования
            ValidateLifetime = true,
            // установка ключа безопасности
            IssuerSigningKey = JwtAuthOptions.GetRefreshTokenSymmetricSecurityKey(),
            // валидация ключа безопасности
            ValidateIssuerSigningKey = true,
         };
}).AddJwtBearer("access-jwt-token", (options)=>
{
    options.Audience = JwtAuthOptions.Audience;
    options.Authority = JwtAuthOptions.Issuer;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
        {
            
            // указывает, будет ли валидироваться издатель при валидации токена
            ValidateIssuer = true,
            // строка, представляющая издателя
            ValidIssuer = JwtAuthOptions.Issuer,
            // будет ли валидироваться потребитель токена
            ValidateAudience = true,
            // установка потребителя токена
            ValidAudience = JwtAuthOptions.Audience,
            // будет ли валидироваться время существования
            ValidateLifetime = true,
            // установка ключа безопасности
            IssuerSigningKey = JwtAuthOptions.GetAccessTokenSymmetricSecurityKey(),
            // валидация ключа безопасности
            ValidateIssuerSigningKey = true,
         };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions(){KeepAliveTimeout=TimeSpan.FromSeconds(4)});





app.MapControllers();

var websocketHandler = new WebSocketHandler();
app.Map("/ws", async (context)=>
{
    await websocketHandler.HandleConnection(context);
});


app.Run();