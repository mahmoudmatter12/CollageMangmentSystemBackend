using CollageManagementSystem.Core.Entities.department;

namespace CollageMangmentSystem.Core.DTO.Responses.course;
public class courseResponseDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public int? Semester { get; set; }
    public bool IsOpen { get; set; } = false;
    public string? DepName { get; set; }
    public Guid? DepartmentId { get; set; }
    public List<string>? PrerequisiteCourses { get; set; }
    public List<Guid> PrerequisiteCourseIds { get; set; } = new List<Guid>();


}