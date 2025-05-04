using Core.Entities.Quizzes;

public class QuizQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public List<string> Answers { get; set; } = new();
    public int Marks { get; set; }
}
