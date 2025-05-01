using CollageMangmentSystem.Core.DTO.Responses.course;
using CollageMangmentSystem.Core.Entities.department;
using System.ComponentModel.DataAnnotations;

namespace CollageMangmentSystem.Core.Entities.course
{
    public class Course : BaseEntity
    {

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

        public courseResponseDto ToCourseResponseDto()
        {
            // Assuming you want to convert this Course entity to a courseResponseDto
            var courseResponseDto = new courseResponseDto
            {
                Id = this.Id,
                Name = this.Name,
                CreditHours = this.CreditHours,
                Semester = this.Semester,
                IsOpen = this.IsOpen,
                DepartmentId = this.DepartmentId,
                DepName = this.Department?.Name,
                PrerequisiteCourseIds = this.PrerequisiteCourseIds,

            };
            return courseResponseDto;
        }
    }
}