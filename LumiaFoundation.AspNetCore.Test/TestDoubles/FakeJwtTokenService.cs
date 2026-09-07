using System.Security.Claims;
using LumiaFoundation.AspNetCore.Auth.Services;

namespace LumiaFoundation.AspNetCore.Test.TestDoubles;

internal sealed class FakeJwtTokenService : IJwtTokenService
{
    private readonly string _userId;

    public FakeJwtTokenService(string userId)
    {
        _userId = userId;
    }

    public ClaimsPrincipal ValidateAndDecodeToken(string jwtToken)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, _userId) },
            "Bearer");

        return new ClaimsPrincipal(identity);
    }

    public string GetUserIdFromToken(string jwtToken) => _userId;
}
