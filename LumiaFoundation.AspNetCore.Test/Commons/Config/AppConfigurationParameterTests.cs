using LumiaFoundation.AspNetCore.Commons.Config;
using Microsoft.Extensions.Configuration;

namespace LumiaFoundation.AspNetCore.Test.Commons.Config;

public class AppConfigurationParameterTests
{
    [Fact]
    public void Constructor_WhenOptionalKeysAreMissing_UsesDefaultValues()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:expires"] = "15",
                ["MariaDbSettings:major"] = "10",
                ["MariaDbSettings:minor"] = "6",
                ["MariaDbSettings:patch"] = "12"
            })
            .Build();

        // Act
        var parameters = new AppConfigurationParameter(configuration);

        // Assert
        Assert.Equal("LumiaSoftwareAPI", parameters.JwtParameter.JwtValidIssuer);
        Assert.Equal("https://localhost:5001", parameters.JwtParameter.JwtValidAudience);
        Assert.Equal("LumiaSoftwareSecretKey113211162023!!!!", parameters.JwtParameter.JwtSecret);
        Assert.Equal(15, parameters.JwtParameter.JwtExpiresMin);
        Assert.Equal("172.17.0.2", parameters.MariaDbParameter.DbHost);
        Assert.Equal("3306", parameters.MariaDbParameter.DbPort);
        Assert.Equal("example-user", parameters.MariaDbParameter.DbUser);
        Assert.Equal("my_cool_secret", parameters.MariaDbParameter.DbPassword);
        Assert.Equal("financasDb", parameters.MariaDbParameter.DbDatabaseName);
        Assert.Equal(10, parameters.MariaDbParameter.MariaDbMajorVersion);
        Assert.Equal(6, parameters.MariaDbParameter.MariaDbMinorVersion);
        Assert.Equal(12, parameters.MariaDbParameter.MariaDbPatchVersion);
    }

    [Fact]
    public void Constructor_WhenKeysArePresent_UsesConfiguredValues()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:validIssuer"] = "issuer",
                ["JwtSettings:validAudience"] = "audience",
                ["JwtSettings:expires"] = "30",
                ["JWTSECRET"] = "secret",
                ["DBHOST"] = "host",
                ["DBPORT"] = "1234",
                ["DBUSER"] = "user",
                ["DBPASSWORD"] = "password",
                ["MariaDbSettings:major"] = "11",
                ["MariaDbSettings:minor"] = "0",
                ["MariaDbSettings:patch"] = "2"
            })
            .Build();

        // Act
        var parameters = new AppConfigurationParameter(configuration);

        // Assert
        Assert.Equal("issuer", parameters.JwtParameter.JwtValidIssuer);
        Assert.Equal("audience", parameters.JwtParameter.JwtValidAudience);
        Assert.Equal("secret", parameters.JwtParameter.JwtSecret);
        Assert.Equal(30, parameters.JwtParameter.JwtExpiresMin);
        Assert.Equal("host", parameters.MariaDbParameter.DbHost);
        Assert.Equal("1234", parameters.MariaDbParameter.DbPort);
        Assert.Equal("user", parameters.MariaDbParameter.DbUser);
        Assert.Equal("password", parameters.MariaDbParameter.DbPassword);
        Assert.Equal(11, parameters.MariaDbParameter.MariaDbMajorVersion);
        Assert.Equal(0, parameters.MariaDbParameter.MariaDbMinorVersion);
        Assert.Equal(2, parameters.MariaDbParameter.MariaDbPatchVersion);
    }
}
