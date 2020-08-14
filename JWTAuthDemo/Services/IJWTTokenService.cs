using JWTAuthDemo.Model;
using System.Security.Claims;

namespace JWTAuthDemo.Services
{
    public interface IJWTTokenService
    {
        string GenerateJSONWebToken(UserModel userinfo);
        string GetUserNameFromToken(ClaimsIdentity identity);
    }
}