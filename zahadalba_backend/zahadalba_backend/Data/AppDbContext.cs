using Microsoft.EntityFrameworkCore;
using zahadalba_backend.Data.Entities;

namespace zahadalba_backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(t => t.ExpiresAt);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(t => t.AbsoluteExpiresAt);
        }
    }
}
