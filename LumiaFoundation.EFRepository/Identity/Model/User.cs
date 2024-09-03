using Microsoft.AspNetCore.Identity;

namespace LumiaFoundation.EFRepository.Identity.Model
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}