using AIRelief.Controllers;
using AIRelief.Models;
using AIRelief.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Tests;

public class LessonControllerTests
{
    [Fact]
    public async Task Index_FirstTimeUser_StartsLessonAndCreatesActiveLesson()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var user = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.User, group.ID);
        await AddLessonQuestionsAsync(fixture.Context);
        fixture.SetCurrentUser(user);

        var controller = fixture.InitializeController(new LessonController(fixture.Context, fixture.UserManager));

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Lesson", view.ViewName);
        var activeLesson = await fixture.Context.ActiveLessons.SingleAsync(al => al.UserID == user.ID);
        Assert.Equal(6, activeLesson.TotalQuestions);
    }

    [Fact]
    public async Task SubmitAnswer_RecordsUserQuestionAndUpdatesStatistics()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var user = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.User, group.ID);
        var question = await AddQuestionAsync(fixture.Context, "Test Question 1", QuestionCategory.CausalReasoning, "A");
        fixture.Context.ActiveLessons.Add(new ActiveLesson
        {
            UserID = user.ID,
            QuestionIds = question.ID.ToString(),
            Categories = QuestionCategory.CausalReasoning,
            CurrentIndex = 0,
            ResultsJson = "[]"
        });
        await fixture.Context.SaveChangesAsync();
        fixture.SetCurrentUser(user);

        var controller = fixture.InitializeController(new LessonController(fixture.Context, fixture.UserManager));

        var result = await controller.SubmitAnswer(question.ID, "A");

        var json = JsonResultAssertions.ToJsonElement(Assert.IsType<JsonResult>(result));
        Assert.True(json.GetProperty("success").GetBoolean());
        Assert.True(json.GetProperty("isCorrect").GetBoolean());

        var attempt = await fixture.Context.UserQuestions.SingleAsync(uq => uq.UserID == user.ID && uq.QuestionID == question.ID);
        Assert.True(attempt.AnsweredCorrectly);

        var stats = await fixture.Context.UserStatistics.SingleAsync(s => s.UserId == user.ID);
        Assert.Equal(1, stats.CausalReasoningAttempts);
        Assert.Equal(1, stats.CausalReasoningPassed);
    }

    [Fact]
    public async Task LessonSummary_RemovesActiveLessonAndSetsCompletionTime()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var user = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.User, group.ID);
        var question = await AddQuestionAsync(fixture.Context, "Test Question 1", QuestionCategory.CausalReasoning, "A");
        fixture.Context.UserStatistics.Add(new UserStatistics
        {
            UserId = user.ID,
            CausalReasoningAttempts = 1,
            CausalReasoningPassed = 1,
            CausalReasoningWeightedAverage = 1m,
            OverallWeightedAverage = 1m
        });
        fixture.Context.ActiveLessons.Add(new ActiveLesson
        {
            UserID = user.ID,
            QuestionIds = question.ID.ToString(),
            Categories = QuestionCategory.CausalReasoning,
            CurrentIndex = 1,
            ResultsJson = $"[{{\"questionId\":{question.ID},\"category\":\"{QuestionCategory.CausalReasoning}\",\"correct\":true}}]"
        });
        await fixture.Context.SaveChangesAsync();
        fixture.SetCurrentUser(user);

        var controller = fixture.InitializeController(new LessonController(fixture.Context, fixture.UserManager));

        var result = await controller.LessonSummary();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LessonSummaryViewModel>(view.Model);
        Assert.Single(model.SessionResults);
        Assert.False(await fixture.Context.ActiveLessons.AnyAsync(al => al.UserID == user.ID));

        var updatedUser = await fixture.Context.Users.SingleAsync(u => u.ID == user.ID);
        Assert.NotNull(updatedUser.DatetimeOfLastQuestionAttempt);
    }

    private static async Task AddLessonQuestionsAsync(AIReliefContext context)
    {
        foreach (var category in new[]
                 {
                     QuestionCategory.CausalReasoning,
                     QuestionCategory.CognitiveReflection,
                     QuestionCategory.ConfidenceCalibration,
                     QuestionCategory.Metacognition,
                     QuestionCategory.ReadingComprehension,
                     QuestionCategory.ShortTermMemory
                 })
        {
            context.Questions.Add(BuildQuestion($"{category} Test Question", category, "A"));
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Question> AddQuestionAsync(AIReliefContext context, string mainText, string category, string correctAnswer)
    {
        var question = BuildQuestion(mainText, category, correctAnswer);
        context.Questions.Add(question);
        await context.SaveChangesAsync();
        return question;
    }

    private static Question BuildQuestion(string mainText, string category, string correctAnswer)
    {
        return new Question
        {
            MainText = mainText,
            QuestionText = mainText,
            Option1 = "A",
            Option2 = "B",
            CorrectAnswer = correctAnswer,
            Category = category,
            ExplanationText = "Explanation"
        };
    }
}