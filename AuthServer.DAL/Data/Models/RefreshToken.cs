namespace AuthServer.DAL.Data.Models
{
    public class RefreshToken : Entity
    {
        public long UserServiceProfileId { get; set; }
        public UserServiceProfile UserServiceProfile { get; set; }
        public string Token { get; set; }
    }
}
