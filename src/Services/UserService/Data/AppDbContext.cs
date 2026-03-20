using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using UserService.Models;

namespace UserService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                // Unique constraints — prevent duplicate username or email
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                // Default role at DB level
                entity.Property(u => u.Role).HasDefaultValue("User");
            });
        }
    }
}