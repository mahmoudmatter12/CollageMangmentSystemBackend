using CollageMangmentSystem.Core.Entities.course;

namespace CollageMangmentSystem.Core.Interfaces
{
    public interface ICourseReposatory<T> : IRepository<Course> where T : class
    {
        // fun that take a list of course ids and return a list of course names
        Task<List<string>> GetCourseNamesByIds(List<Guid> courseIds);

    }
}