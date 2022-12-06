using Microsoft.AspNetCore.Identity;

namespace AuthServer.Models
{
    public class User : IdentityUser, IEntity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Family { get; set; }
        public DateTime Created { get; set; }
        public string RefreshToken { get; set; }
        public DateTime LastCreatedRefreshToken { get; set; }
    }
}
