namespace AuthServer.DTO.User
{
    public class UserGridDto : UserAuthDto
    {
        public int Id { get; set; }   
        public bool IsConfirmed { get; set; }        
    }
}
