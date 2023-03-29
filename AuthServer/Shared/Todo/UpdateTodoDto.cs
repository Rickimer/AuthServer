namespace AuthServer.Shared.Todo
{
    public class UpdateTodoDto
    {
        public int? Id { get; set; }
        public string? Title { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
