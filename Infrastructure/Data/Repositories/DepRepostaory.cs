using CollageMangmentSystem.Core.Entities.department;
using CollageMangmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CollageMangmentSystem.Core.Interfaces.department
{
    public class DepRepostaory<T> : IDepRepostaory<T> where T : class

    {
        protected readonly ApplicationDbContext _context;
        public DepRepostaory(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Department entity)
        {
            await _context.Set<Department>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Department entity)
        {
            _context.Set<Department>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Set<Department>().ToListAsync();
        }

        public async Task<IEnumerable<Department>> GetAllAsyncPaged(int pageNumber, int pageSize, Func<IQueryable<Department>, IQueryable<Department>>? include = null)
        {
            var query = _context.Set<Department>().AsQueryable();

            if (include != null)
            {
                query = include(query);
            }

            return await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(Guid id)
        {
            return await _context.Set<Department>().FindAsync(id);
        }

        public async Task<string> GetDepartmentName(Guid? departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            return department != null ? department.Name : string.Empty;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

    }
}