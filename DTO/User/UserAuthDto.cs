namespace AuthServer.DTO.User
{
    public class UserAuthDto: UserBaseDto
    {
        public IList<string>? Roles { get; set; }        
        public string? Password { get; set; }
    }
}
