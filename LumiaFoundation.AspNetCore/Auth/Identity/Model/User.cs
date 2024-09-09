using Microsoft.AspNetCore.Identity;

namespace LumiaFoundation.AspNetCore.Auth.Identity.Model
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}