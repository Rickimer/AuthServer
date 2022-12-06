namespace AuthServer.DTO.User
{
    public class UserDto : UserBaseDto
    {
        public string RefreshToken { get; set; }
        public DateTime LastCreatedRefreshToken { get; set; }
        public string PasswordHash { get; set; }
    }
}
