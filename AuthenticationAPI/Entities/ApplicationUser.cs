using Microsoft.AspNetCore.Identity;

namespace AuthenticationAPI.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
