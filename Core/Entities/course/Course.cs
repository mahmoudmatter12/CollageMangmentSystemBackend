using CollageMangmentSystem.Core.Entities.department;
using System.ComponentModel.DataAnnotations;

namespace CollageMangmentSystem.Core.Entities.course
{
    public class Course
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 10)]
        public int CreditHours { get; set; }

        [Range(1, 10)]
        public int? Semester { get; set; }

        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public bool IsOpen { get; set; } = false;

        public List<Guid> PrerequisiteCourseIds { get; set; } = new List<Guid>();
        public List<Course> PrerequisiteCourses { get; set; } = new List<Course>();

        public List<string> PrerequisiteCoursesNames()
        {
            return PrerequisiteCourses?.Select(c => c.Name).ToList() ?? new List<string>();
        }

        // Assuming the department name is fetched from a related entity
        public string GetDepartmentName(Department dep)
        {
            // Replace this with the actual logic to fetch the department name
            return dep.Name ?? string.Empty;
        }
    }
}