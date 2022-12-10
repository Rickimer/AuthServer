using Microsoft.AspNetCore.Identity;

namespace AuthServer.Models
{
    public class User : IdentityUser
    {        
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime LastCreatedRefreshToken { get; set; }
    }
}
