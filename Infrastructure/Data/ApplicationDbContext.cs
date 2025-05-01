using Microsoft.AspNetCore.Http;  // For IHttpContextAccessor
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;     // For accessing user claims
using CollageMangmentSystem.Core.Entities;
using CollageMangmentSystem.Core.Entities.course;
using CollageMangmentSystem.Core.Entities.department;
using System.Linq.Expressions;

namespace CollageMangmentSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Inject IHttpContextAccessor to get current user
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Course> Courses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique email index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Soft delete query filter (applies to all entities inheriting from BaseEntity)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType)))
            {
                // Create a parameter expression
                var parameter = Expression.Parameter(entityType.ClrType, "e");

                // Create property access expression
                var property = Expression.Property(parameter, "IsDeleted");

                // Create constant expression for false
                var falseConstant = Expression.Constant(false);

                // Create equality expression
                var equalExpression = Expression.Equal(property, falseConstant);

                // Create lambda expression
                var lambda = Expression.Lambda(equalExpression, parameter);

                // Apply the filter
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        public override int SaveChanges()
        {
            SetAuditProperties();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditProperties();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetAuditProperties()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";
            var currentTime = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = currentTime;
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.UpdatedAt = currentTime;
                    entry.Entity.UpdatedBy = currentUser;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = currentTime;
                    entry.Entity.UpdatedBy = currentUser;
                }
                else if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = currentTime;
                    entry.Entity.DeletedBy = currentUser;
                }
            }
        }
    }

    // Optional interface for explicit soft delete implementation
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string? DeletedBy { get; set; }
    }
}