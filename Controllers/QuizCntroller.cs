using CollageManagementSystem.Core;
using Core.Entities.Quizzes;
using Microsoft.AspNetCore.Mvc;

namespace CollageManagementSystem
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizRepository _quizRepository;

        public QuizController(IQuizRepository quizRepository)
        {
            _quizRepository = quizRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuiz(Guid id)
        {
            var quiz = await _quizRepository.GetQuizByIdAsync(id);
            return quiz != null ? Ok(quiz) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _quizRepository.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Quiz quiz)
        {
            if (!QuizValidator.IsValidQuiz(quiz, out var error))
                return BadRequest(error);

            await _quizRepository.AddQuizAsync(quiz);
            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Quiz quiz)
        {
            if (quiz.Id != id)
                return BadRequest("Quiz ID mismatch.");

            if (!QuizValidator.IsValidQuiz(quiz, out var error))
                return BadRequest(error);

            await _quizRepository.UpdateQuizAsync(quiz);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _quizRepository.DeleteQuizAsync(id);
            return NoContent();
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveQuizzes()
        {
            var active = await _quizRepository.GetActiveQuizzesAsync();
            return Ok(active);
        }
    }

}