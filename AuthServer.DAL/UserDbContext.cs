using AuthServer.DAL.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.DAL
{
    public class UserDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<User> AuthUsers => Set<User>();        
        public DbSet<Traffic> Traffic => Set<Traffic>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<UserServiceProfile> UserServiceProfiles => Set<UserServiceProfile>();

        public UserDbContext()
        {
            Database.EnsureCreated();
        }
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());

            builder.Entity<UserProfile>(b =>
            {
                b.HasIndex(e => new { e.Login, e.AuthSystemId }).IsUnique();
            });
            builder.Entity<UserServiceProfile>(b =>
            {
                b.HasIndex(e => new { e.UserProfileId, e.ServiceId }).IsUnique();
            });

        }

        internal class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
            }
        }
    }
}
