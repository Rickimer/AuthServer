namespace AuthServer.Models
{
    public class Todo : IEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}
