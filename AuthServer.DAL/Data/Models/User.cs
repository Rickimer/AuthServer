using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.DAL.Data.Models
{
    public class User : IdentityUser
    {
        [ForeignKey("UserProfile")]
        public ulong? UserProfileId { get; set; }
        public UserProfile? UserProfile { get; set; }
    }
}
