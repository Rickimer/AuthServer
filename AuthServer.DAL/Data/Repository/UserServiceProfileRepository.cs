using AuthServer.DAL.Data.Models;

namespace AuthServer.DAL.Data.Repository
{
    public class UserServiceProfileRepository : GeneralRepository<UserServiceProfile, UserDbContext>
    {
        public UserServiceProfileRepository(UserDbContext context) : base(context)
        {
        }
    }
}
