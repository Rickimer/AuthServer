using AuthServer.BLL.AppConst;

namespace AuthServer.BLL.DTO.User
{
    public class UserServiceProfileDto
    {
        public ConsumeServiceEnumDto ServiceId { get; set; }
        public long UserProfileId { get; set; }
        public UserProfileDto UserProfile { get; set; }
    }
}
