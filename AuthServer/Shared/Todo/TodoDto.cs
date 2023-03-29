namespace AuthServer.Shared.Todo
{
    public class TodoDto
    {
        public ulong Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool isCompleted { get; set; }
    }
}
