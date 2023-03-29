namespace AuthServer.BLL.DTO.Todo
{
    public class BllCreateTodoDto
    {
        public ulong UserId { get; set; }
        public string? Title { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
