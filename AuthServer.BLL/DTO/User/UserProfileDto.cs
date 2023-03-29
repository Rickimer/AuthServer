using AuthServer.BLL.AppConst;

namespace AuthServer.BLL.DTO.User
{
    public class UserProfileDto
    {
        public string Login { get; set; }
        public string? Email { get; set; }
        public AuthSystemEnumDto AuthSystemId { get; set; }
        public string UserId { get; set; }
        public ICollection<UserServiceProfileDto> UserServiceProfilesDto { get; set; } = new List<UserServiceProfileDto>();
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
