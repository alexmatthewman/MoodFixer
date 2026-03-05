using AIRelief.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIRelief.Controllers
{
    [Authorize]
    public class LessonController : Controller
    {
        // The six non-trial categories used in lessons
        private static readonly string[] LessonCategories =
        {
            QuestionCategory.CausalReasoning,
            QuestionCategory.CognitiveReflection,
            QuestionCategory.ConfidenceCalibration,
            QuestionCategory.Metacognition,
            QuestionCategory.ReadingComprehension,
            QuestionCategory.ShortTermMemory
        };

        // Maps QueryFrequency enum to days
        private static int QueryFrequencyToDays(QueryFrequency freq) => freq switch
        {
            QueryFrequency.Daily     => 1,
            QueryFrequency.Weekly    => 7,
            QueryFrequency.BiWeekly  => 14,
            QueryFrequency.Monthly   => 30,
            QueryFrequency.Quarterly => 90,
            _                        => 7
        };

        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LessonController(AIReliefContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ?????????????????????????????????????????????????????????????
        // GET /Lesson  – routing hub: decides which page to show
        // ?????????????????????????????????????????????????????????????
        public async Task<IActionResult> Index()
        {
            var (appUser, error) = await GetAppUserWithStatsAsync();
            if (error != null) return error;

            // Case 1 – active lesson already in progress ? resume it
            var active = await _context.ActiveLessons
                .FirstOrDefaultAsync(al => al.UserID == appUser.ID);
            if (active != null)
                return await ResumeLesson(appUser, active);

            // Case 2 – never attempted
            if (appUser.DatetimeOfLastQuestionAttempt == null)
                return await StartLesson(appUser, isFirstEver: true);

            // Determine how many days the user's query frequency corresponds to
            var freqDays = appUser.QueryFrequency.HasValue
                ? QueryFrequencyToDays(appUser.QueryFrequency.Value)
                : (appUser.Group != null ? QueryFrequencyToDays(appUser.Group.QueryFrequency) : 7);

            var nextDue = appUser.DatetimeOfLastQuestionAttempt.Value.AddDays(freqDays);
            var now = DateTime.UtcNow;

            // Case 3 – enough time has passed ? start new lesson
            if (now >= nextDue)
                return await StartLesson(appUser, isFirstEver: false);

            // Case 4 – too early; show countdown popup then statistics
            var timeUntilNext = nextDue - now;
            TempData["HoursUntilNext"]   = (int)timeUntilNext.TotalHours;
            TempData["MinutesUntilNext"] = timeUntilNext.Minutes;
            return RedirectToAction(nameof(UserStatistics));
        }

        // ?????????????????????????????????????????????????????????????
        // GET /Lesson/Lesson  – the actual question set page
        // ?????????????????????????????????????????????????????????????
        public async Task<IActionResult> Lesson()
        {
            var (appUser, error) = await GetAppUserWithStatsAsync();
            if (error != null) return error;

            var active = await _context.ActiveLessons
                .FirstOrDefaultAsync(al => al.UserID == appUser.ID);
            if (active != null)
                return await ResumeLesson(appUser, active);

            return RedirectToAction(nameof(Index));
        }

        // ?????????????????????????????????????????????????????????????
        // POST /Lesson/SubmitAnswer
        // ?????????????????????????????????????????????????????????????
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int questionId, string selectedAnswer)
        {
            var question = await _context.Questions.FirstOrDefaultAsync(q => q.ID == questionId);
            if (question == null)
                return Json(new { success = false, message = "Question not found" });

            bool isCorrect = selectedAnswer == question.CorrectAnswer;

            var (appUser, _) = await GetAppUserWithStatsAsync();
            if (appUser == null)
                return Json(new { success = false, message = "User not found" });

            var active = await _context.ActiveLessons
                .FirstOrDefaultAsync(al => al.UserID == appUser.ID);
            if (active == null)
                return Json(new { success = false, message = "No active lesson found" });

            // Persist UserQuestion record
            var existing = await _context.UserQuestions
                .FirstOrDefaultAsync(uq => uq.UserID == appUser.ID && uq.QuestionID == questionId);

            if (existing == null)
            {
                _context.UserQuestions.Add(new UserQuestion
                {
                    UserID             = appUser.ID,
                    QuestionID         = questionId,
                    DateFirstAttempted = DateTime.UtcNow,
                    DateLastAttempted  = DateTime.UtcNow,
                    AnsweredCorrectly  = isCorrect
                });
            }
            else
            {
                existing.DateLastAttempted = DateTime.UtcNow;
                existing.AnsweredCorrectly = isCorrect;
            }

            // Update UserStatistics
            await UpdateStatisticsAsync(appUser, question.Category, isCorrect, active, questionId);

            // Advance the active lesson index
            active.CurrentIndex++;
            var questionIds = active.QuestionIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            bool isLastQuestion = active.CurrentIndex >= questionIds.Length;

            _context.ActiveLessons.Update(active);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isCorrect,
                correctionText  = question.ExplanationText,
                correctionImage = question.ExplanationImage,
                isLastQuestion,
                nextQuestionUrl = isLastQuestion
                    ? Url.Action("LessonSummary")
                    : Url.Action("NextQuestion"),
                currentIndex = active.CurrentIndex
            });
        }

        // ?????????????????????????????????????????????????????????????
        // GET /Lesson/NextQuestion  – returns next question partial
        // ?????????????????????????????????????????????????????????????
        public async Task<IActionResult> NextQuestion()
        {
            var (appUser, error) = await GetAppUserWithStatsAsync();
            if (error != null) return error;

            var active = await _context.ActiveLessons
                .FirstOrDefaultAsync(al => al.UserID == appUser.ID);
            if (active == null)
                return RedirectToAction("LessonSummary");

            var questionIds = active.QuestionIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out int v) ? v : -1)
                .Where(v => v > 0).ToArray();

            var categories = active.Categories
                .Split('|', StringSplitOptions.RemoveEmptyEntries);

            var currentIndex = active.CurrentIndex;

            if (currentIndex >= questionIds.Length)
                return RedirectToAction("LessonSummary");

            var question = await _context.Questions.FindAsync(questionIds[currentIndex]);
            if (question == null)
                return RedirectToAction("LessonSummary");

            string category = currentIndex < categories.Length
                ? categories[currentIndex]
                : question.Category ?? string.Empty;

            ViewBag.Category = category;
            ViewBag.CurrentIndex = currentIndex;
            ViewBag.TotalQuestions = questionIds.Length;

            return PartialView("_LessonQuestionPartial", question);
        }

        // ?????????????????????????????????????????????????????????????
        // GET /Lesson/LessonSummary
        // ?????????????????????????????????????????????????????????????
        public async Task<IActionResult> LessonSummary()
        {
            var (appUser, error) = await GetAppUserWithStatsAsync();
            if (error != null) return error;

            var active = await _context.ActiveLessons
                .FirstOrDefaultAsync(al => al.UserID == appUser.ID);

            List<ActiveLessonResult> sessionResults;
            if (active != null)
            {
                sessionResults = DeserializeResults(active.ResultsJson);
                // Lesson is complete – remove the active lesson record
                _context.ActiveLessons.Remove(active);
                // Record completion time so the cooldown starts from now
                appUser.DatetimeOfLastQuestionAttempt = DateTime.UtcNow;
                _context.Users.Update(appUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                sessionResults = new List<ActiveLessonResult>();
            }

            // Build session-level CategoryResult list
            var categoryResults = sessionResults
                .GroupBy(r => r.Category)
                .Select(g => new CategoryResult
                {
                    Category  = g.Key,
                    Attempted = g.Count(),
                    Correct   = g.Count(r => r.Correct)
                })
                .OrderBy(r => r.Category)
                .ToList();

            // Build per-question summary with global success rates
            var answeredQuestionIds = sessionResults.Select(r => r.QuestionId).Where(id => id > 0).ToList();
            var questionLookup = answeredQuestionIds.Count > 0
                ? await _context.Questions
                    .Where(q => answeredQuestionIds.Contains(q.ID))
                    .ToDictionaryAsync(q => q.ID)
                : new Dictionary<int, Question>();

            var questionSummaryItems = sessionResults
                .Where(r => r.QuestionId > 0 && questionLookup.ContainsKey(r.QuestionId))
                .Select(r =>
                {
                    var q = questionLookup[r.QuestionId];
                    int? globalSuccess = q.AttemptsShown > 0
                        ? (int)Math.Round((double)(q.AttemptsCorrect ?? 0) / q.AttemptsShown.Value * 100)
                        : (int?)null;
                    return new QuestionSummaryItem
                    {
                        QuestionId          = r.QuestionId,
                        QuestionText        = q.MainText ?? q.QuestionText ?? string.Empty,
                        Category            = r.Category,
                        UserCorrect         = r.Correct,
                        GlobalSuccessPercent = globalSuccess
                    };
                })
                .ToList();

            // Determine how many lessons the user has completed
            // Each lesson contributes one attempt per category (6 categories), so total attempts / 6 gives lesson count.
            // Ensure stats are loaded
            if (appUser.Statistics == null)
            {
                appUser.Statistics = await _context.UserStatistics
                    .FirstOrDefaultAsync(s => s.UserId == appUser.ID);
            }

            int totalAttempts = appUser.Statistics == null ? 0 :
                appUser.Statistics.CausalReasoningAttempts +
                appUser.Statistics.CognitiveReflectionAttempts +
                appUser.Statistics.ConfidenceCalibrationAttempts +
                appUser.Statistics.MetacognitionAttempts +
                appUser.Statistics.ReadingComprehensionAttempts +
                appUser.Statistics.ShortTermMemoryAttempts;

            // Each lesson = 6 questions (one per category); round to nearest whole lesson
            int lessonsCompleted = (int)Math.Round((double)totalAttempts / 6);

            var vm = new LessonSummaryViewModel
            {
                SessionResults       = categoryResults,
                QuestionSummaryItems = questionSummaryItems,
                Statistics           = appUser.Statistics,
                LessonsCompleted     = lessonsCompleted
            };

            return View(vm);
        }

        // ?????????????????????????????????????????????????????????????
        // GET /Lesson/UserStatistics  – shown when too early for next set
        // ?????????????????????????????????????????????????????????????
        public async Task<IActionResult> UserStatistics()
        {
            var (appUser, error) = await GetAppUserWithStatsAsync();
            if (error != null) return error;

            if (appUser.Statistics == null)
            {
                appUser.Statistics = await _context.UserStatistics
                    .FirstOrDefaultAsync(s => s.UserId == appUser.ID);
            }

            return View(appUser.Statistics);
        }

        // ?????????????????????????????????????????????????????????????
        // Private helpers
        // ?????????????????????????????????????????????????????????????

        private async Task<IActionResult> StartLesson(User appUser, bool isFirstEver)
        {
            // Pick one unseen question per lesson category
            var seenIds = await _context.UserQuestions
                .Where(uq => uq.UserID == appUser.ID)
                .Select(uq => uq.QuestionID)
                .ToListAsync();

            var items = new List<LessonQuestionItem>();
            var rng = new Random();

            foreach (var cat in LessonCategories)
            {
                var candidates = await _context.Questions
                    .Where(q => q.Category == cat && !seenIds.Contains(q.ID))
                    .ToListAsync();

                if (candidates.Count == 0)
                {
                    // All seen – pick any question in this category
                    candidates = await _context.Questions
                        .Where(q => q.Category == cat)
                        .ToListAsync();
                }

                if (candidates.Count == 0)
                    continue;

                var picked = candidates[rng.Next(candidates.Count)];
                items.Add(new LessonQuestionItem
                {
                    Question   = picked,
                    Category   = cat,
                    IndexInSet = items.Count
                });
            }

            if (items.Count == 0)
            {
                TempData["Error"] = "No questions are available for your lesson. Please contact your administrator.";
                return RedirectToAction("Index", "Home");
            }

            // Persist the chosen set as an ActiveLesson row
            var active = new ActiveLesson
            {
                UserID       = appUser.ID,
                QuestionIds  = string.Join(",", items.Select(i => i.Question.ID)),
                Categories   = string.Join("|", items.Select(i => i.Category)),
                CurrentIndex = 0,
                ResultsJson  = "[]",
                CreatedAt    = DateTime.UtcNow,
                IsFirstEver  = seenIds.Count == 0
            };
            _context.ActiveLessons.Add(active);
            await _context.SaveChangesAsync();

            ViewBag.Questions = items.Select(i => i.Question).ToList();
            return View("Lesson", active);
        }

        private async Task<IActionResult> ResumeLesson(User appUser, ActiveLesson active)
        {
            var ids = active.QuestionIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out int v) ? v : -1)
                .Where(v => v > 0).ToArray();

            if (ids.Length == 0 || active.CurrentIndex >= ids.Length)
            {
                // Corrupt / already finished – clean up and redirect
                _context.ActiveLessons.Remove(active);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Load only the question rows this lesson needs
            var questions = await _context.Questions
                .Where(q => ids.Contains(q.ID))
                .ToListAsync();

            ViewBag.Questions = questions;
            return View("Lesson", active);
        }

        private async Task UpdateStatisticsAsync(User appUser, string category, bool isCorrect, ActiveLesson active, int questionId)
        {
            var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == appUser.ID);
            if (stats == null)
            {
                stats = new UserStatistics { UserId = appUser.ID };
                _context.UserStatistics.Add(stats);
            }

            // Record this result in the ActiveLesson
            var results = DeserializeResults(active.ResultsJson);
            results.Add(new ActiveLessonResult { QuestionId = questionId, Category = category ?? string.Empty, Correct = isCorrect });
            active.ResultsJson = JsonSerializer.Serialize(results);

            // Update global per-question success counters
            var q = await _context.Questions.FindAsync(questionId);
            if (q != null)
            {
                q.AttemptsShown = (q.AttemptsShown ?? 0) + 1;
                if (isCorrect)
                    q.AttemptsCorrect = (q.AttemptsCorrect ?? 0) + 1;
            }

            // Increment attempts and pass counts per category
            switch (category)
            {
                case QuestionCategory.CausalReasoning:
                    stats.CausalReasoningAttempts++;
                    if (isCorrect) stats.CausalReasoningPassed++;
                    stats.CausalReasoningWeightedAverage = stats.CalculateWeightedAverage(
                        stats.CausalReasoningPassed, stats.CausalReasoningAttempts);
                    break;

                case QuestionCategory.CognitiveReflection:
                    stats.CognitiveReflectionAttempts++;
                    if (isCorrect) stats.CognitiveReflectionPassed++;
                    stats.CognitiveReflectionWeightedAverage = stats.CalculateWeightedAverage(
                        stats.CognitiveReflectionPassed, stats.CognitiveReflectionAttempts);
                    break;

                case QuestionCategory.ConfidenceCalibration:
                    stats.ConfidenceCalibrationAttempts++;
                    if (isCorrect) stats.ConfidenceCalibrationPassed++;
                    stats.ConfidenceCalibrationWeightedAverage = stats.CalculateWeightedAverage(
                        stats.ConfidenceCalibrationPassed, stats.ConfidenceCalibrationAttempts);
                    break;

                case QuestionCategory.Metacognition:
                    stats.MetacognitionAttempts++;
                    if (isCorrect) stats.MetacognitionPassed++;
                    stats.MetacognitionWeightedAverage = stats.CalculateWeightedAverage(
                        stats.MetacognitionPassed, stats.MetacognitionAttempts);
                    break;

                case QuestionCategory.ReadingComprehension:
                    stats.ReadingComprehensionAttempts++;
                    if (isCorrect) stats.ReadingComprehensionPassed++;
                    stats.ReadingComprehensionWeightedAverage = stats.CalculateWeightedAverage(
                        stats.ReadingComprehensionPassed, stats.ReadingComprehensionAttempts);
                    break;

                case QuestionCategory.ShortTermMemory:
                    stats.ShortTermMemoryAttempts++;
                    if (isCorrect) stats.ShortTermMemoryPassed++;
                    stats.ShortTermMemoryWeightedAverage = stats.CalculateWeightedAverage(
                        stats.ShortTermMemoryPassed, stats.ShortTermMemoryAttempts);
                    break;
            }

            stats.CalculateOverallWeightedAverage();
            stats.LastUpdated = DateTime.UtcNow;
        }

        private static List<ActiveLessonResult> DeserializeResults(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<ActiveLessonResult>();
            try
            {
                return JsonSerializer.Deserialize<List<ActiveLessonResult>>(json)
                       ?? new List<ActiveLessonResult>();
            }
            catch
            {
                return new List<ActiveLessonResult>();
            }
        }

        private async Task<(User user, IActionResult error)> GetAppUserWithStatsAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return (null, Forbid());

            var appUser = await _context.Users
                .Include(u => u.Statistics)
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.Email == identityUser.Email);

            if (appUser == null)
                return (null, Forbid());

            return (appUser, null);
        }
    }
}
