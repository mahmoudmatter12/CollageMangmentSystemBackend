// Import necessary namespaces
using CollageMangmentSystem.Core.Entities;  // For accessing your entity classes
using Microsoft.EntityFrameworkCore;        // For EF Core functionality

namespace CollageMangmentSystem.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }

    }
}
