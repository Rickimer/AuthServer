namespace AuthServer.BLL.DTO.Todo
{
    public class BllTodoDto
    {
        public ulong Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool isCompleted { get; set; }
    }
}
