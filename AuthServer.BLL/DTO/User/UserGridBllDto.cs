namespace AuthServer.BLL.DTO.User
{
    public class UserGridBllDto : UserAuthDto
    {
        public int Id { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
