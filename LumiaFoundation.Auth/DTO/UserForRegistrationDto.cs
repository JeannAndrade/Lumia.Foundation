using System.ComponentModel.DataAnnotations;
using LumiaFoundation.Auth.Identity.Model;

namespace LumiaFoundation.Auth.DTO;

public class UserForRegistrationDto
{
    [Required(ErrorMessage = "FirstName is required")]
    public required string FirstName { get; init; }

    [Required(ErrorMessage = "LastName is required")]
    public required string LastName { get; init; }

    [Required(ErrorMessage = "Username is required")]
    public required string UserName { get; init; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
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
