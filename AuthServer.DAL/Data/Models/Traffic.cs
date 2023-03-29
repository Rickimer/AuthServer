namespace AuthServer.DAL.Data.Models
{
    public class Traffic : Entity
    {
        public DateTime DateEvent { get; set; }
        public string? IP { get; set; }
        public string? Path { get; set; }
    }
}
