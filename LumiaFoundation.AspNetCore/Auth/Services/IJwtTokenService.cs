using System.Security.Claims;

namespace LumiaFoundation.AspNetCore.Auth.Services
{
    public interface IJwtTokenService
    {
        ClaimsPrincipal ValidateAndDecodeToken(string jwtToken);
        string GetUserIdFromToken(string jwtToken);
    }
}