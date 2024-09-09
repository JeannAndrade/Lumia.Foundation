using System.ComponentModel.DataAnnotations;
using LumiaFoundation.AspNetCore.Auth.Identity.Model;

namespace LumiaFoundation.AspNetCore.Auth.DTO
{
    public class UserForRegistrationDto
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }

        [Required(ErrorMessage = "Username is required")]
        public string? UserName { get; init; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public ICollection<string>? Roles { get; init; }

        public User ConvertToUser()
        {
            return new User
            {
                FirstName = FirstName,
                LastName = LastName,
                UserName = UserName,
                Email = Email,
                PhoneNumber = PhoneNumber
            };
        }
    }
}