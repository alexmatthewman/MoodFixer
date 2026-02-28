using AIRelief.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AIRelief.Controllers
{
    public class TrialController : Controller
    {
        private readonly AIReliefContext _context;

        public TrialController(AIReliefContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get all questions first, then randomize in memory
            var allQuestions = await _context.Questions.ToListAsync();

            if (allQuestions.Count < 3)
            {
                TempData["Error"] = "Not enough questions available for trial.";
                return RedirectToAction("Index", "Home");
            }

            // Randomize and take 3
            var questions = allQuestions
                .OrderBy(x => Guid.NewGuid())
                .Take(3)
                .ToList();

            // Store question IDs in session for tracking
            HttpContext.Session.SetString("TrialQuestions",
                string.Join(",", questions.Select(q => q.ID)));
            HttpContext.Session.SetInt32("TrialCurrentIndex", 0);
            HttpContext.Session.SetInt32("TrialScore", 0);

            return View(questions.First());
        }
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int questionId, string selectedAnswer)
        {
            Console.WriteLine($"Processing question ID: {questionId}");

            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.ID == questionId);

            if (question == null)
            {
                Console.WriteLine("Question not found");
                return Json(new { success = false, message = "Question not found" });
            }

            Console.WriteLine($"Question found: {question.MainText?.Substring(0, 50)}...");
            Console.WriteLine($"Explanation text: {question.ExplanationText}");
            Console.WriteLine($"Explanation image: {question.ExplanationImage}");

            // Check if the selected answer is correct
            bool isCorrect = selectedAnswer == question.CorrectAnswer;

            Console.WriteLine($"Selected: '{selectedAnswer}', Correct: '{question.CorrectAnswer}', IsCorrect: {isCorrect}");

            // Update score if correct
            if (isCorrect)
            {
                var currentScore = HttpContext.Session.GetInt32("TrialScore") ?? 0;
                HttpContext.Session.SetInt32("TrialScore", currentScore + 1);
            }

            // Get current index and increment
            var currentIndex = HttpContext.Session.GetInt32("TrialCurrentIndex") ?? 0;
            var nextIndex = currentIndex + 1;
            HttpContext.Session.SetInt32("TrialCurrentIndex", nextIndex);

            // Check if this was the last question
            bool isLastQuestion = nextIndex >= 3;

            var response = new
            {
                success = true,
                isCorrect = isCorrect,
                correctionText = question.ExplanationText, // Always include, let JS decide
                correctionImage = question.ExplanationImage, // Always include, let JS decide
                isLastQuestion = isLastQuestion,
                nextQuestionUrl = isLastQuestion ? Url.Action("Results") : Url.Action("NextQuestion"),
                currentIndex = nextIndex,
                questionId = questionId // Include for debugging
            };

            Console.WriteLine($"Sending response: isCorrect={response.isCorrect}, correctionText='{response.correctionText}'");

            return Json(response);
        }

        public async Task<IActionResult> NextQuestion()
        {
            var questionIds = HttpContext.Session.GetString("TrialQuestions")?.Split(',');
            var currentIndex = HttpContext.Session.GetInt32("TrialCurrentIndex") ?? 0;

            if (questionIds == null || currentIndex >= questionIds.Length)
            {
                return RedirectToAction("Results");
            }

            var questionId = int.Parse(questionIds[currentIndex]);
            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.ID == questionId);

            if (question == null)
            {
                return RedirectToAction("Results");
            }

            return PartialView("_QuestionPartial", question);
        }

        public IActionResult Results()
        {
            var score = HttpContext.Session.GetInt32("TrialScore") ?? 0;
            var percentage = (score * 100) / 3;

            var model = new TrialResultViewModel
            {
                Score = score,
                TotalQuestions = 3,
                Percentage = percentage,
                Message = GetScoreMessage(percentage),
                ShowTrainingSuggestion = percentage < 70
            };

            // Clear session
            HttpContext.Session.Remove("TrialQuestions");
            HttpContext.Session.Remove("TrialCurrentIndex");
            HttpContext.Session.Remove("TrialScore");

            return View(model);
        }

        private string GetScoreMessage(int percentage)
        {
            return percentage switch
            {
                >= 90 => "Excellent! Your critical thinking skills are sharp.",
                >= 70 => "Good work! You have solid reasoning abilities.",
                >= 50 => "Not bad, but there's room for improvement.",
                _ => "Your critical thinking skills need strengthening."
            };
        }
    }
}