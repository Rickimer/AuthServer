namespace AuthServer.Models
{
    public class Traffic : IEntity
    {
        public int Id { get; set; }
        public DateTime DateEvent { get; set; }
        public string? IP { get; set; }
        public string? Path { get; set; }
    }
}
