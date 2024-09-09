using LumiaFoundation.AspNetCore.Auth.DTO;
using Microsoft.AspNetCore.Identity;

namespace LumiaFoundation.AspNetCore.Auth.Services
{
    public interface IAuthenticationService
    {
        Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration);
        Task<bool> ValidateUser(UserForAuthenticationDto userForAuth);
        Task<string> CreateToken();
    }
}