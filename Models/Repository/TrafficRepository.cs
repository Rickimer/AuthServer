namespace AuthServer.Models.Repository
{
    public class TrafficRepository : GeneralRepository<Traffic, UserDbContext>
    {
        public TrafficRepository(UserDbContext context) : base(context)
        {
        }
    }
}
