using AuthServer.DAL.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.DAL.Data.Models
{
    /// <summary>
    /// Extended by  unique identification = Login + AuthSystem.
    /// </summary>
    public class UserProfile : Entity
    {
        public string Login { get; set; }
        public string? Email { get; set; }
        public AuthSystemEnum AuthSystemId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }
        private ICollection<UserServiceProfile>? _userServiceProfiles { get; set; }
        public ICollection<UserServiceProfile> UserServiceProfiles
        {
            get
            {
                return this._userServiceProfiles ?? (this._userServiceProfiles = new List<UserServiceProfile>());
            }
            set { this._userServiceProfiles = value; }
        }

        public long? ExternalAuthId { get; set; }
        public string? Avatar_url { get; set; }
        public string? Gravatar_id { get; set; }
        public string? Url { get; set; }
        public string? Html_url { get; set; }
        public string? Type { get; set; }
        public bool? Syte_admin { get; set; }
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Blog { get; set; }
        public string? Location { get; set; }
        public bool? Hireable { get; set; }
        public string? Bio { get; set; }
    }
}
