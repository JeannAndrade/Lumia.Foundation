using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LumiaFoundation.AspNetCore.Commons.Config;
using Microsoft.IdentityModel.Tokens;

namespace LumiaFoundation.AspNetCore.Auth.Services
{
    public class JwtTokenService(IAppConfigurationParameter configuration) : IJwtTokenService
    {
        private readonly IAppConfigurationParameter _configuration = configuration;

        public ClaimsPrincipal ValidateAndDecodeToken(string jwtToken)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.JwtParameter.JwtSecret)),
                ValidateLifetime = true,
                ValidIssuer = _configuration.JwtParameter.JwtValidIssuer,
                ValidAudience = _configuration.JwtParameter.JwtValidAudience
            };

            var principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out _);
            return principal;
        }

        public string GetUserIdFromToken(string jwtToken)
        {

            var claimsPrincipal = ValidateAndDecodeToken(jwtToken);

            foreach (Claim c in claimsPrincipal.Claims)
            {
                if (c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                    return c.Value;
            }

            throw new ArgumentException("Não foi possível obter o Id do usuário a partir do token");
        }
    }
}