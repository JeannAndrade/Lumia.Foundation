using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LumiaFoundation.AspNetCore.Auth.DTO;
using LumiaFoundation.AspNetCore.Auth.Identity.Model;
using LumiaFoundation.AspNetCore.Commons.Config;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace LumiaFoundation.AspNetCore.Auth.Services
{
    internal sealed class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IAppConfigurationParameter _configuration;
        private User? _user;

        public AuthenticationService(UserManager<User> userManager, IAppConfigurationParameter configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration)
        {
            var user = userForRegistration.ConvertToUser();
            var result = await _userManager.CreateAsync(user, userForRegistration.Password);

            if (result.Succeeded)
                await _userManager.AddToRolesAsync(user, userForRegistration.Roles);

            return result;
        }

        public async Task<bool> ValidateUser(UserForAuthenticationDto userForAuth)
        {
            _user = await _userManager.FindByNameAsync(userForAuth.UserName);
            var result = _user != null && await _userManager.CheckPasswordAsync(_user, userForAuth.Password);

            //if (!result)
            //    _logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong user name or password.");

            return result;
        }

        public async Task<string> CreateToken()
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_configuration.JwtParameter.JwtSecret);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, _user.UserName), new(ClaimTypes.NameIdentifier, _user.Id) };
            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: _configuration.JwtParameter.JwtValidIssuer,
                audience: _configuration.JwtParameter.JwtValidAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_configuration.JwtParameter.JwtExpiresMin),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }
    }
}