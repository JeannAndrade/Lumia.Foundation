using System.Security.Claims;

namespace LumiaFoundation.Auth.Services;

public interface IJwtTokenService
{
    ClaimsPrincipal ValidateAndDecodeToken(string jwtToken);
    string GetUserIdFromToken(string jwtToken);
}
