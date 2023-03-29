using AuthServer.DAL.Data.Models;

namespace AuthServer.DAL.Data.Repository
{
    public class RefreshTokenRepository : GeneralRepository<RefreshToken, UserDbContext>
    {
        public RefreshTokenRepository(UserDbContext context) : base(context)
        {
        }
    }
}
