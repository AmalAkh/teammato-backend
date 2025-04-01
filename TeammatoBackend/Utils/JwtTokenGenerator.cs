using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TeammatoBackend.Utils;

public class JwtTokenGenerator
{
    public static JwtSecurityToken GenerateAccessToken(List<Claim> ?claims)
    {
        return new JwtSecurityToken(
                issuer:JwtAuthOptions.Issuer, 
                audience:JwtAuthOptions.Audience, 
                claims:claims,
                expires:DateTime.UtcNow.Add(TimeSpan.FromDays(365)),
                signingCredentials: new SigningCredentials(
                    JwtAuthOptions.GetAccessTokenSymmetricSecurityKey(), 
                    SecurityAlgorithms.HmacSha256
                )
                            
        );
    }

    public static JwtSecurityToken GenerateRefreshToken(List<Claim> ?claims)
    {
        return new JwtSecurityToken(
                issuer:JwtAuthOptions.Issuer, 
                audience:JwtAuthOptions.Audience, 
                claims:claims,
                expires:DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
                signingCredentials: new SigningCredentials(
                    JwtAuthOptions.GetRefreshTokenSymmetricSecurityKey(), 
                    SecurityAlgorithms.HmacSha256
                )
                            
        );
    }
   
}