using Microsoft.EntityFrameworkCore;
using StudyAuthApp.WebApi.Models;

namespace StudyAuthApp.WebApi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) 
            : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(e => e.Username).IsUnique();
            modelBuilder.Entity<User>().HasIndex(e => e.Email).IsUnique();
        }
    }
}
