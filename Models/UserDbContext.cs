using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Models
{
    public class UserDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<User> AuthUsers => Set<User>();
        public DbSet<Todo> Todos => Set<Todo>();
        public DbSet<Traffic> Traffic => Set<Traffic>();

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
        }

        internal class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                builder.Property(u => u.FirstName).HasMaxLength(255);
                builder.Property(u => u.LastName).HasMaxLength(255);
                builder.Property(u => u.Family).HasMaxLength(255);
                builder.Property(u => u.RefreshToken).HasMaxLength(255);
            }
        }
    }
}
