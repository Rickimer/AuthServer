namespace AuthServer.Shared
{
    public class UserPostDto
    {
        public IList<string>? Roles { get; set; }
        public string? Password { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
    }
}
