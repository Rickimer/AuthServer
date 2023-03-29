using AuthServer.DAL.Data.Models;

namespace AuthServer.DAL.Data.Repository
{
    public class UserProfileRepository : GeneralRepository<UserProfile, UserDbContext>
    {
        public UserProfileRepository(UserDbContext context) : base(context)
        {
        }
    }
}
