using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LumiaFoundation.AspNetCore.Auth.Services;
using LumiaFoundation.AspNetCore.Commons.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LumiaFoundation.AspNetCore.Test.Auth.Services;

public class JwtTokenServiceTests
{
    [Fact]
    public void ValidateAndDecodeToken_WhenTokenIsValid_ReturnsClaimsPrincipal()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);
        var token = CreateToken(configuration, new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Name, "demo")
        });

        // Act
        var principal = service.ValidateAndDecodeToken(token);

        // Assert
        Assert.Equal("demo", principal.Identity?.Name);
        Assert.Contains(principal.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == "user-123");
    }

    [Fact]
    public void GetUserIdFromToken_WhenTokenContainsNameIdentifier_ReturnsUserId()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);
        var token = CreateToken(configuration, new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-456")
        });

        // Act
        var userId = service.GetUserIdFromToken(token);

        // Assert
        Assert.Equal("user-456", userId);
    }

    [Fact]
    public void GetUserIdFromToken_WhenTokenDoesNotContainNameIdentifier_ThrowsArgumentException()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var service = new JwtTokenService(configuration);
        var token = CreateToken(configuration, new[]
        {
            new Claim(ClaimTypes.Name, "demo")
        });

        // Act
        var exception = Assert.Throws<ArgumentException>(() => service.GetUserIdFromToken(token));

        // Assert
        Assert.Contains("Id do usuário", exception.Message);
    }

    private static AppConfigurationParameter CreateConfiguration()
    {
        var configurationRoot = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:validIssuer"] = "issuer",
                ["JwtSettings:validAudience"] = "audience",
                ["JwtSettings:expires"] = "15",
                ["JWTSECRET"] = "super-secret-value-which-is-long-enough",
                ["MariaDbSettings:major"] = "10",
                ["MariaDbSettings:minor"] = "6",
                ["MariaDbSettings:patch"] = "12"
            })
            .Build();

        return new AppConfigurationParameter(configurationRoot);
    }

    private static string CreateToken(AppConfigurationParameter configuration, IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.JwtParameter.JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: configuration.JwtParameter.JwtValidIssuer,
            audience: configuration.JwtParameter.JwtValidAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
