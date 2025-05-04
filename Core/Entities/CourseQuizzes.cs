using CollageMangmentSystem.Core.Entities.course;
using Core.Entities.Quizzes;

namespace CollageMangmentSystem.Core.Entities
{
    public class CourseQuizzes : BaseEntity
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public Guid QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;
    }
    
}