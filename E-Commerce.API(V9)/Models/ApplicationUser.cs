using Microsoft.AspNetCore.Identity;

namespace E_Commerce.API_V9_.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FName { get; set; } = string.Empty;   
        public string LName { get; set; } = string.Empty;
        public string? Address { get; set; }
    }
}
