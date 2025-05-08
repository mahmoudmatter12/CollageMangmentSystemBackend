using CollageManagementSystem.Core;
using Core.Entities.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace CollageMangmentSystem.Infrastructure.Data.Repositories
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Quiz?> GetQuizByIdAsync(Guid id)
        {
            return await _context.Quizzes.FindAsync(id);
        }

        public async Task<IEnumerable<Quiz>> GetAllQuizzesAsync()
        {
            return await _context.Quizzes.ToListAsync();
        }

        public async Task AddQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuizAsync(Quiz quiz)
        {
            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuizAsync(Guid id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Quiz>> GetActiveQuizzesAsync()
        {
            return await _context.Quizzes.Where(q => q.IsActive).ToListAsync();
        }

        public async Task<QuizResultDto> CheckAnsweringQuizAsync(QuizSubmissionDto submission)
        {
            // 1. Fetch the quiz with questions
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == submission.QuizId);

            if (quiz == null)
                throw new ArgumentException("Quiz not found.");

            var questionResults = new List<QuestionResultDto>();
            int obtainedMarks = 0;

            foreach (var question in quiz.Questions)
            {
                var submittedAnswer = submission.Answers
                    .FirstOrDefault(a => a.QuestionId == question.Id);

                bool isCorrect = submittedAnswer != null &&
                                 submittedAnswer.SelectedAnswerIndex == question.CorrectAnswerIndex;

                int awardedMarks = isCorrect ? question.Marks : 0;
                obtainedMarks += awardedMarks;

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = question.Id,
                    IsCorrect = isCorrect,
                    AwardedMarks = awardedMarks
                });
            }

            // 2. Construct the result DTO
            var result = new QuizResultDto
            {
                QuizId = quiz.Id,
                ObtainedMarks = obtainedMarks,
                IsPassed = obtainedMarks >= quiz.PassingMarks,
                QuestionsResult = questionResults
            };

            return result;
        }
        public async Task<IEnumerable<QuizQuestion>> GetAllQuestionsAsync(Guid QuizID)
        {
            var QuizQuestions = await _context.QuizQuestions
                .Where(q => q.QuizId == QuizID)
                .ToListAsync();
            return QuizQuestions;
        }

        public async Task<IEnumerable<QuizWithQuestionsResponseDto>> GetQuizWithQuestionsAsync(Guid quizId)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            var TotalMarks = quiz?.Questions.Sum(q => q.Marks);

            if (quiz == null)
                throw new ArgumentException("Quiz not found.");

            var quizDto = new QuizWithQuestionsResponseDto
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description ?? "No Description",
                Duration = quiz.Duration,
                PassingMarks = quiz.PassingMarks,
                IsActive = quiz.IsActive,
                StartDate = quiz.StartDate ?? DateTime.MinValue,
                EndDate = quiz.EndDate ?? DateTime.MinValue,
                TotalMarks = quiz.Questions.Sum(q => q.Marks),
                TotalQuestions = quiz.Questions.Count,
                Questions = quiz.Questions.Select(q => new QuizQuestionResponseDto
                {
                    QuestionText = q.QuestionText,
                    Type = q.Type,
                    Marks = q.Marks,
                    Answers = q.Answers,
                    CorrectAnswerIndex = q.CorrectAnswerIndex
                }).ToList()
            };

            return new List<QuizWithQuestionsResponseDto> { quizDto };
        }
    }

}