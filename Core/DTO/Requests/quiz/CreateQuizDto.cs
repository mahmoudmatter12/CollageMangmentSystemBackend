public class QuizDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Duration { get; set; }
    public int PassingMarks { get; set; }
    public List<QuizQuestionDto> Questions { get; set; } = new();
}
