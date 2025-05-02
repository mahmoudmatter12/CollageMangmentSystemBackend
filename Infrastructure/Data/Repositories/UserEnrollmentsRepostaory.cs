using CollageMangmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CollageManagementSystem.Core.Entities.userEnrollments
{
    public class UserEnrollmentsRepostaory<T> : IUserEnrollments<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        public UserEnrollmentsRepostaory(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddUserEnrollment(T userEnrollment)
        {
            _context.Set<T>().Add(userEnrollment);
            return _context.SaveChangesAsync();
        }

        public Task<bool> DeleteUserEnrollment(Guid id)
        {
            var userEnrollment = _context.Set<T>().Find(id);
            if (userEnrollment == null)
            {
                return Task.FromResult(false);
            }

            _context.Set<T>().Remove(userEnrollment);
            return _context.SaveChangesAsync().ContinueWith(t => t.Result > 0);
        }

        public Task<List<UserEnrollments>> GetAllUserEnrollments()
        {
            return _context.UserEnrollments.ToListAsync();
        }

        public Task<List<UserEnrollments>> GetUserEnrollmentById(Guid id)
        {
            return _context.UserEnrollments
                .Where(ue => ue.UserId == id)
                .ToListAsync();
        }
    }
}
