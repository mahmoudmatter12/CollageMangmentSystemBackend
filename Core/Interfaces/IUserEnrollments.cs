namespace CollageManagementSystem.Core.Entities.userEnrollments
{
    public interface IUserEnrollments <T> where T : class
    {
        Task<List<UserEnrollments>> GetUserEnrollmentById(Guid id);
        Task<List<UserEnrollments>> GetAllUserEnrollments();
        Task AddUserEnrollment(T userEnrollment);
        Task<bool> DeleteUserEnrollment(Guid id);
    }
}