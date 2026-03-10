using AIRelief.Controllers;
using AIRelief.Models;
using AIRelief.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Tests;

public class QuestionsControllerTests
{
    [Fact]
    public async Task Create_SavesQuestion_ForSystemAdmin()
    {
        await using var fixture = new ControllerTestFixture();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        fixture.SetCurrentUser(admin);

        var controller = fixture.InitializeController(new QuestionsController(fixture.Context, fixture.UserManager));
        var question = BuildQuestion("Test Question 1", QuestionCategory.CausalReasoning);

        controller.TryValidateModel(question);

        var result = await controller.Create(question);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.Index), redirect.ActionName);
        Assert.True(await fixture.Context.Questions.AnyAsync(q => q.MainText == "Test Question 1"));
    }

    [Fact]
    public async Task Edit_UpdatesQuestion_ForSystemAdmin()
    {
        await using var fixture = new ControllerTestFixture();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        fixture.SetCurrentUser(admin);
        var question = await AddQuestionAsync(fixture.Context, "Test Question 1", QuestionCategory.CausalReasoning);

        var controller = fixture.InitializeController(new QuestionsController(fixture.Context, fixture.UserManager));
        question.MainText = "Test Question 1 Updated";
        question.Option2 = "Updated option";

        var result = await controller.Edit(question.ID, question);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.Index), redirect.ActionName);
        var updated = await fixture.Context.Questions.SingleAsync(q => q.ID == question.ID);
        Assert.Equal("Test Question 1 Updated", updated.MainText);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesQuestion_ForSystemAdmin()
    {
        await using var fixture = new ControllerTestFixture();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        fixture.SetCurrentUser(admin);
        var question = await AddQuestionAsync(fixture.Context, "Test Question 1", QuestionCategory.CognitiveReflection);

        var controller = fixture.InitializeController(new QuestionsController(fixture.Context, fixture.UserManager));

        var result = await controller.DeleteConfirmed(question.ID);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(QuestionsController.Index), redirect.ActionName);
        Assert.False(await fixture.Context.Questions.AnyAsync(q => q.ID == question.ID));
    }

    [Fact]
    public async Task Index_ForUserLevelAccess_ReturnsForbid()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var user = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.User, group.ID);
        fixture.SetCurrentUser(user);

        var controller = fixture.InitializeController(new QuestionsController(fixture.Context, fixture.UserManager));

        var result = await controller.Index();

        Assert.IsType<ForbidResult>(result);
    }

    private static Question BuildQuestion(string mainText, string category)
    {
        return new Question
        {
            MainText = mainText,
            QuestionText = mainText,
            Option1 = "A",
            Option2 = "B",
            CorrectAnswer = "A",
            Category = category,
            ExplanationText = "Explanation"
        };
    }

    private static async Task<Question> AddQuestionAsync(AIReliefContext context, string mainText, string category)
    {
        var question = BuildQuestion(mainText, category);
        context.Questions.Add(question);
        await context.SaveChangesAsync();
        return question;
    }
}