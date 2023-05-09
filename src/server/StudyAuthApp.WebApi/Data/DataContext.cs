using Microsoft.EntityFrameworkCore;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) 
            : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<UserToken> UserTokens { get; set; }

        public DbSet<ResetToken> ResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(e => e.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(e => e.Email).IsUnique();

            modelBuilder.Entity<ResetToken>().HasIndex(e => e.Token).IsUnique();
        }
    }
}
