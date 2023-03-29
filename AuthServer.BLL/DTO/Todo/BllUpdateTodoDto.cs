namespace AuthServer.BLL.DTO.Todo
{
    public class BllUpdateTodoDto
    {
        public ulong Id;
        public string Title = string.Empty;
        public ulong UserId { get; set; }
    }
}
