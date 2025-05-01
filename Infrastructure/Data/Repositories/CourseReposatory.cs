using CollageMangmentSystem.Core.Entities.course;
using CollageMangmentSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollageMangmentSystem.Infrastructure.Data.Repositories
{
    public class CourseReposatory<T> : ICourseReposatory<T> where T : class

    {
        protected readonly ApplicationDbContext _context;
        public CourseReposatory(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Course entity)
        {
            await _context.Set<Course>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Course entity)
        {
            _context.Set<Course>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await _context.Set<Course>().ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetAllAsyncPaged(int pageNumber, int pageSize, Func<IQueryable<Course>, IQueryable<Course>>? include = null)
        {
            var query = _context.Set<Course>().AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Course>().FindAsync(id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.Set<Course>().CountAsync();
        }

        public async Task SoftDeleteAsync(Course entity, Guid deletedById)
        {
            // Assuming T has a property called IsDeleted
            var property = typeof(T).GetProperty("IsDeleted");
            var deletedByProperty = typeof(T).GetProperty("DeletedBy");
            var deletedAtProperty = typeof(T).GetProperty("DeletedAt");
            if (property != null && deletedByProperty != null && deletedAtProperty != null)
            {
                property.SetValue(entity, true);
                deletedByProperty.SetValue(entity, deletedById);
                deletedAtProperty.SetValue(entity, DateTime.UtcNow);
                await UpdateAsync(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Entity does not have the required properties for soft deletion.");
            }
        }

        public Task<List<string>> GetCourseNamesByIds(List<Guid> courseIds)
        {
            var courseNames = _context.Courses
                .Where(c => courseIds.Contains(c.Id))
                .Select(c => c.Name)
                .ToListAsync();

            return courseNames;
        }
    }
}