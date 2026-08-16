using System.ComponentModel.DataAnnotations;

namespace LumiaFoundation.AspNetCore.Auth.DTO
{
    public class UserForAuthenticationDto
    {
        [Required(ErrorMessage = "User name is required")]
        public required string UserName { get; init; }
        [Required(ErrorMessage = "Password name is required")]
        public required string Password { get; init; }
    }
}