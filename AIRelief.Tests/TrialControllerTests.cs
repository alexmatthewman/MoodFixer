using AIRelief.Controllers;
using AIRelief.Models;
using AIRelief.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIRelief.Tests;

public class TrialControllerTests
{
    [Fact]
    public async Task Index_WithThreeTrialQuestions_StartsTrialSession()
    {
        await using var fixture = new ControllerTestFixture();
        await AddTrialQuestionsAsync(fixture.Context);
        var controller = fixture.InitializeController(new TrialController(fixture.Context));

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var question = Assert.IsType<Question>(view.Model);
        Assert.Equal(QuestionCategory.Trial, question.Category);

        var session = controller.HttpContext.Session;
        Assert.NotNull(session.GetString("TrialQuestions"));
        Assert.Equal(0, session.GetInt32("TrialCurrentIndex"));
        Assert.Equal(0, session.GetInt32("TrialScore"));
    }

    [Fact]
    public async Task SubmitAnswer_CorrectAnswer_IncrementsScore()
    {
        await using var fixture = new ControllerTestFixture();
        var question = await AddTrialQuestionAsync(fixture.Context, "Trial Question 1", "A");
        var controller = fixture.InitializeController(new TrialController(fixture.Context));

        controller.HttpContext.Session.SetString("TrialQuestions", question.ID.ToString());
        controller.HttpContext.Session.SetInt32("TrialCurrentIndex", 0);
        controller.HttpContext.Session.SetInt32("TrialScore", 0);

        var result = await controller.SubmitAnswer(question.ID, "A");

        var json = JsonResultAssertions.ToJsonElement(Assert.IsType<JsonResult>(result));
        Assert.True(json.GetProperty("success").GetBoolean());
        Assert.True(json.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(1, controller.HttpContext.Session.GetInt32("TrialScore"));
        Assert.Equal(1, controller.HttpContext.Session.GetInt32("TrialCurrentIndex"));
    }

    [Fact]
    public async Task Results_ClearsTrialSession()
    {
        await using var fixture = new ControllerTestFixture();
        var controller = fixture.InitializeController(new TrialController(fixture.Context));

        controller.HttpContext.Session.SetString("TrialQuestions", "1,2,3");
        controller.HttpContext.Session.SetInt32("TrialCurrentIndex", 3);
        controller.HttpContext.Session.SetInt32("TrialScore", 2);

        var result = controller.Results();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TrialResultViewModel>(view.Model);
        Assert.Equal(2, model.Score);
        Assert.Null(controller.HttpContext.Session.GetString("TrialQuestions"));
        Assert.Null(controller.HttpContext.Session.GetInt32("TrialCurrentIndex"));
        Assert.Null(controller.HttpContext.Session.GetInt32("TrialScore"));
    }

    private static async Task AddTrialQuestionsAsync(AIReliefContext context)
    {
        for (var i = 1; i <= 3; i++)
        {
            context.Questions.Add(new Question
            {
                MainText = $"Trial Question {i}",
                QuestionText = $"Trial Question {i}",
                Option1 = "A",
                Option2 = "B",
                CorrectAnswer = "A",
                Category = QuestionCategory.Trial,
                ExplanationText = "Explanation"
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Question> AddTrialQuestionAsync(AIReliefContext context, string mainText, string correctAnswer)
    {
        var question = new Question
        {
            MainText = $"{mainText} with sufficient length for controller logging output.",
            QuestionText = $"{mainText} with sufficient length for controller logging output.",
            Option1 = "A",
            Option2 = "B",
            CorrectAnswer = correctAnswer,
            Category = QuestionCategory.Trial,
            ExplanationText = "Explanation"
        };

        context.Questions.Add(question);
        await context.SaveChangesAsync();
        return question;
    }
}