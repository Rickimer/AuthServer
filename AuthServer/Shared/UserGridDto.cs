namespace AuthServer.Shared
{
    public class UserGridDto
    {
        public int Id { get; set; }
        public bool IsConfirmed { get; set; }
        public IList<string>? Roles { get; set; }
        public string? Password { get; set; }
        public string? Login { get; set; }
        public string? Email { get; set; }
    }
}
