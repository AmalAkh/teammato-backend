using System.Text;
using Microsoft.IdentityModel.Tokens;
using dotenv.net;
using dotenv.net.Utilities;

namespace TeammatoBackend.Utils;

public static class JwtAuthOptions
{
    // Constants for the JWT token issuer and audience
    public static readonly string Issuer = "teammato";
    public static readonly string Audience = "teammato-user";
    // Private static variables for the refresh and access token secret keys.
    // These will be set by reading values from the .env file.
    private static readonly string RefreshTokenSecretKey;
    private static readonly string AccessTokenSecretKey;

    // Static constructor for the JwtAuthOptions class.
    static JwtAuthOptions()
    {
        // Load environment variables from .env file
        DotEnv.Load();

        // Access the environment variables
        RefreshTokenSecretKey = EnvReader.GetStringValue("REFRESH_TOKEN_SECRET_KEY");
        AccessTokenSecretKey = EnvReader.GetStringValue("ACCESS_TOKEN_SECRET_KEY");
    }

    // Method to get the symmetric security key used to sign the refresh token
    public static SymmetricSecurityKey GetRefreshTokenSymmetricSecurityKey()
    {
        // Check if the refresh token secret key is missing or empty
        if (string.IsNullOrEmpty(RefreshTokenSecretKey))
        {
            // If the key is missing, throw an exception
            throw new InvalidOperationException("Refresh Token Secret Key is missing from the .env file.");
        }
        // Return the SymmetricSecurityKey used for signing the refresh token.
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RefreshTokenSecretKey));
    }

    // Method to get the symmetric security key used to sign the access token
    public static SymmetricSecurityKey GetAccessTokenSymmetricSecurityKey()
    {
        // Check if the access token secret key is missing or empty
        if (string.IsNullOrEmpty(AccessTokenSecretKey))
        {
            // If the key is missing, throw an exception
            throw new InvalidOperationException("Access Token Secret Key is missing from the .env file.");
        }
        // Return the SymmetricSecurityKey used for signing the access token.
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AccessTokenSecretKey));
    }
}