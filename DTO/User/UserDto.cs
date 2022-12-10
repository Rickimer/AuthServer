namespace AuthServer.DTO.User
{
    public class UserDto : UserBaseDto
    {
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime LastCreatedRefreshToken { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
    }
}
