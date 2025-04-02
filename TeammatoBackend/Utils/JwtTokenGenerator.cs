using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TeammatoBackend.Utils;

public class JwtTokenGenerator
{
    // Method for generating an Access Token
    // Accepts a list of claims (statements) and creates a JWT token with the specified parameters
    public static JwtSecurityToken GenerateAccessToken(List<Claim> ?claims)
    {
        return new JwtSecurityToken(
                issuer:JwtAuthOptions.Issuer,       // Indicates the issuer of the token
                audience:JwtAuthOptions.Audience,   // Indicates the audience of the token
                claims:claims,                      // Statements to be attached to the token
                expires:DateTime.UtcNow.Add(TimeSpan.FromDays(365)),        // Token expiration time (for 365 days)
                signingCredentials: new SigningCredentials(
                    JwtAuthOptions.GetAccessTokenSymmetricSecurityKey(),    // Get a symmetric key to sign the token
                    SecurityAlgorithms.HmacSha256                           // Algorithm for token signature (HMAC with SHA-256)
                )
        );
    }

    // Method for generating an Refresh Token
    // Accepts a list of claims (statements) and creates a JWT token with the specified parameters
    public static JwtSecurityToken GenerateRefreshToken(List<Claim> ?claims)
    {
        return new JwtSecurityToken(
                issuer:JwtAuthOptions.Issuer,       // Indicates the issuer of the token
                audience:JwtAuthOptions.Audience,   // Indicates the audience of the token
                claims:claims,                      // Statements to be attached to the token
                expires:DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),      // Token expiration time (for 30 minutes)
                signingCredentials: new SigningCredentials(
                    JwtAuthOptions.GetRefreshTokenSymmetricSecurityKey(),   // Get a symmetric key to sign the token
                    SecurityAlgorithms.HmacSha256                           // Algorithm for token signature (HMAC with SHA-256)
                )
        );
    }
}
