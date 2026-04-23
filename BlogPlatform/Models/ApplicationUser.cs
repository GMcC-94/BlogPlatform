using Microsoft.AspNetCore.Identity;

namespace BlogPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
    }
}
