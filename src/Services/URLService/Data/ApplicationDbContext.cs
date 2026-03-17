using Microsoft.EntityFrameworkCore;
using URLService.Entities;

namespace URLService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Table for storing URL metadata
        public DbSet<Url> Urls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensure ShortCode is unique for fast indexing
            modelBuilder.Entity<Url>()
                .HasIndex(u => u.ShortCode)
                .IsUnique();
        }
    }
}