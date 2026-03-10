using AIRelief.Controllers;
using AIRelief.Models;
using AIRelief.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Tests;

public class UserAccessControllerTests
{
    [Fact]
    public async Task HomeIndex_ForUserLevelAccess_RedirectsToLessonIndex()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var user = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.User, group.ID);
        fixture.SetCurrentUser(user);

        var controller = fixture.InitializeController(new HomeController(fixture.Context, fixture.UserManager));

        var result = await controller.Index();

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Lesson", redirect.ControllerName);
    }

    [Fact]
    public async Task MyQuestions_ForUserLevelAccess_ReturnsOnlyOwnAttempts()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var user1 = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.User, group.ID);
        var user2 = await fixture.CreateAppUserAsync("TestUser2", "testuser2@testgroup.local", AuthLevel.User, group.ID);
        var question1 = await AddQuestionAsync(fixture.Context, "Test Question 1", QuestionCategory.CausalReasoning);
        var question2 = await AddQuestionAsync(fixture.Context, "Test Question 2", QuestionCategory.CognitiveReflection);

        fixture.Context.UserQuestions.AddRange(
            new UserQuestion
            {
                UserID = user1.ID,
                QuestionID = question1.ID,
                DateFirstAttempted = DateTime.UtcNow,
                DateLastAttempted = DateTime.UtcNow,
                AnsweredCorrectly = true
            },
            new UserQuestion
            {
                UserID = user2.ID,
                QuestionID = question2.ID,
                DateFirstAttempted = DateTime.UtcNow,
                DateLastAttempted = DateTime.UtcNow,
                AnsweredCorrectly = false
            });
        await fixture.Context.SaveChangesAsync();

        fixture.SetCurrentUser(user1);
        var controller = fixture.InitializeController(new HomeController(fixture.Context, fixture.UserManager));

        var result = await controller.MyQuestions();

        var view = Assert.IsType<ViewResult>(result);
        var attempts = Assert.IsAssignableFrom<IEnumerable<UserQuestion>>(view.Model);
        Assert.Single(attempts);
        Assert.Equal(user1.ID, attempts.Single().UserID);
    }

    [Fact]
    public async Task GroupAdminIndex_ForUserLevelAccess_ReturnsForbid()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var user = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.User, group.ID);
        fixture.SetCurrentUser(user);

        var controller = fixture.InitializeController(new GroupAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService));

        var result = await controller.Index();

        Assert.IsType<ForbidResult>(result);
    }

    private static async Task<Question> AddQuestionAsync(AIReliefContext context, string mainText, string category)
    {
        var question = new Question
        {
            MainText = mainText,
            QuestionText = mainText,
            Option1 = "A",
            Option2 = "B",
            CorrectAnswer = "A",
            Category = category,
            ExplanationText = "Explanation"
        };

        context.Questions.Add(question);
        await context.SaveChangesAsync();
        return question;
    }
}