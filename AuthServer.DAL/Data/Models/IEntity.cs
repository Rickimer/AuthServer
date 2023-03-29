namespace AuthServer.DAL.Data.Models
{
    public interface IEntity
    {
        public ulong Id { get; set; }
        public DateTime Created { get; set; }
    }
}
