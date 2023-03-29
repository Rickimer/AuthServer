using AuthServer.DAL.Data.Enums;

namespace AuthServer.DAL.Data.Models
{
    /// <summary>        
    /// RefreshTokens and Roles by ConsumeService
    /// </summary>
    public class UserServiceProfile : Entity
    {
        public ConsumeServiceEnum ServiceId { get; set; }
        public ulong UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }

        private ICollection<RefreshToken> _refreshTokens;
        public ICollection<RefreshToken>? RefreshTokens
        {
            get
            {
                return this._refreshTokens ?? (this._refreshTokens = new List<RefreshToken>());
            }
        }
    }
}
