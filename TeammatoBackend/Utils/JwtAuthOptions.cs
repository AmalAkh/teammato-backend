using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TeammatoBackend.Utils;

public static class JwtAuthOptions
{
    public static readonly string Issuer = "teammato";
    public static readonly string Audience = "teammato-user";
    private static readonly string RefreshTokenSecretKey = "snquuyhondmbtmvukgmztgljjsoyqyemtdutfobfujzcvvnthuhstdphniixqlkvqruwywjwobqyfwresaaqtdxcvgldoakkqfapglnqdflqshbugdbfbzgayjfbbywv";
    private static readonly string AccessTokenSecretKey = "SBJRC3bvK7RlFA4JtY7ElPoCmCGcaAkrnXHASNK0jYdQLf0pqvnIoNGdL28fn9tknbNPFjOd8EuKIudgYGhQ9cO2sh2gPRRBLjrpcwyyDlSObVECgVddWmyshGdF7NPJ";

    public static SymmetricSecurityKey GetRefreshTokenSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(RefreshTokenSecretKey));
    }

    public static SymmetricSecurityKey GetAccessTokenSymmetricSecurityKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AccessTokenSecretKey));
    }
   
}