using CollageMangmentSystem.Core.Entities;

namespace Core.Entities.Quizzes
{
    public class Quiz : BaseEntity
    {
        // Quiz MetaData
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Duration { get; set; } // in minutes
        public int PassingMarks { get; set; } = 50;
        public bool IsActive { get; set; } = false;
        public int? MaxAttempts { get; set; } = null; // use null instead of 0 if "unlimited" is implied

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        public int GetTotalMarks()
        {
            return this.Questions.Sum(q => q.Marks);
        }

        public string GetDurationString()
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                var duration = EndDate.Value - StartDate.Value;
                return duration.ToString(@"hh\:mm\:ss");
            }
            return "Forever";
        }


    }


}