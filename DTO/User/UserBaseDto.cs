using System.ComponentModel.DataAnnotations;

namespace AuthServer.DTO.User
{
    public class UserBaseDto
    {
        public string? UserName { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
    }
}
