using AuthServer.DAL.Data.Models;

namespace AuthServer.DAL.Data.Repository
{
    public class TrafficRepository : GeneralRepository<Traffic, UserDbContext>
    {
        public TrafficRepository(UserDbContext context) : base(context)
        {
        }
    }

}
