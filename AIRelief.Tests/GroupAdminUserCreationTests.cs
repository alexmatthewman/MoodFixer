using System.Text;
using AIRelief.Controllers;
using AIRelief.Models;
using AIRelief.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AIRelief.Tests;

public class GroupAdminUserCreationTests
{
    [Fact]
    public async Task CreateUser_CreatesTestUserInTestGroup_AndCleanupDeletesIt()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.GroupAdmin, group.ID);
        fixture.SetCurrentUser(admin);

        var controller = fixture.InitializeController(new GroupAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService));

        var createdEmail = $"testuser2-{Guid.NewGuid():N}@testgroup.local";
        var createdUser = new User
        {
            Name = "TestUser2",
            Email = createdEmail,
            AuthLevel = AuthLevel.User,
            GroupId = group.ID
        };

        controller.TryValidateModel(createdUser);
        Assert.True(controller.ModelState.IsValid);

        var createResult = await controller.CreateUser(group.ID, createdUser, "TempPass123!");

        var redirect = Assert.IsType<RedirectToActionResult>(createResult);
        Assert.Equal(nameof(GroupAdminController.Index), redirect.ActionName);

        var savedUser = await fixture.Context.Users.SingleOrDefaultAsync(u => u.Email == createdEmail);
        Assert.NotNull(savedUser);
        Assert.Equal(group.ID, savedUser.GroupId);
        Assert.True(fixture.IdentityUserExists(createdEmail));

        var removeResult = await controller.RemoveUser(savedUser.ID);

        var removeRedirect = Assert.IsType<RedirectToActionResult>(removeResult);
        Assert.Equal(nameof(GroupAdminController.Index), removeRedirect.ActionName);
        Assert.Null(await fixture.Context.Users.SingleOrDefaultAsync(u => u.Email == createdEmail));
        Assert.False(fixture.IdentityUserExists(createdEmail));
    }

    [Fact]
    public async Task BulkAddUsers_AddsValidRows_AndSkipsDisallowedRows()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync(licenses: 5);
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.GroupAdmin, group.ID);
        await fixture.CreateAppUserAsync("TestUserExisting", "existing@testgroup.local", AuthLevel.User, group.ID);
        fixture.SetCurrentUser(admin);

        var controller = fixture.InitializeController(new GroupAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService));

        const string csv = "Name,Email,AuthLevel,Password\r\n"
            + "TestUser2,testuser2@testgroup.local,User,TempPass123!\r\n"
            + "TestUser3,testuser3@testgroup.local,GroupAdmin,TempPass123!\r\n"
            + "TestUser4,existing@testgroup.local,User,TempPass123!\r\n"
            + "TestUser5,testuser5@testgroup.local,SystemAdmin,TempPass123!\r\n";

        var bytes = Encoding.UTF8.GetBytes(csv);
        using var stream = new MemoryStream(bytes);
        var csvFile = new FormFile(stream, 0, bytes.Length, "csvFile", "users.csv");

        var result = await controller.BulkAddUsers(group.ID, csvFile);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(GroupAdminController.ManageUsers), redirect.ActionName);
        Assert.True(await fixture.Context.Users.AnyAsync(u => u.Email == "testuser2@testgroup.local"));
        Assert.True(await fixture.Context.Users.AnyAsync(u => u.Email == "testuser3@testgroup.local"));
        Assert.False(await fixture.Context.Users.AnyAsync(u => u.Email == "testuser5@testgroup.local"));

        var bulkAdded = Assert.IsType<int>(controller.TempData["BulkAdded"]);
        Assert.Equal(2, bulkAdded);
    }

    [Fact]
    public async Task GroupStatistics_ReturnsOnlyUsersFromTestGroup()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var otherGroup = await fixture.CreateTestGroupAsync("OtherGroup");
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.GroupAdmin, group.ID);
        var groupMember = await fixture.CreateAppUserAsync("TestUser2", "testuser2@testgroup.local", AuthLevel.User, group.ID);
        await fixture.CreateAppUserAsync("TestUser3", "testuser3@other.local", AuthLevel.User, otherGroup.ID);

        fixture.Context.UserStatistics.Add(new UserStatistics
        {
            UserId = groupMember.ID,
            CausalReasoningAttempts = 1,
            CausalReasoningPassed = 1,
            CausalReasoningWeightedAverage = 1m,
            OverallWeightedAverage = 1m
        });
        await fixture.Context.SaveChangesAsync();

        fixture.SetCurrentUser(admin);
        var controller = fixture.InitializeController(new GroupAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService));

        var result = await controller.GroupStatistics();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<GroupStatisticsViewModel>(view.Model);
        Assert.Equal("TestGroup", model.GroupName);
        Assert.Equal(2, model.Members.Count);
        Assert.Contains(model.Members, m => m.Name == "TestUser1");
        Assert.Contains(model.Members, m => m.Name == "TestUser2");
    }

    [Fact]
    public async Task UserQuestions_ReturnsOnlyCurrentGroupsAttempts()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@testgroup.local", AuthLevel.GroupAdmin, group.ID);
        var member = await fixture.CreateAppUserAsync("TestUser2", "testuser2@testgroup.local", AuthLevel.User, group.ID);
        var question = await AddQuestionAsync(fixture.Context, QuestionCategory.CausalReasoning, "Test Question 1");

        fixture.Context.UserQuestions.Add(new UserQuestion
        {
            UserID = member.ID,
            QuestionID = question.ID,
            DateFirstAttempted = DateTime.UtcNow,
            DateLastAttempted = DateTime.UtcNow,
            AnsweredCorrectly = true
        });
        await fixture.Context.SaveChangesAsync();

        fixture.SetCurrentUser(admin);
        var controller = fixture.InitializeController(new GroupAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService));

        var result = await controller.UserQuestions(member.ID);

        var view = Assert.IsType<ViewResult>(result);
        var attempts = Assert.IsAssignableFrom<IEnumerable<UserQuestion>>(view.Model);
        Assert.Single(attempts);
        Assert.Equal(member.ID, attempts.Single().UserID);
    }

    private static async Task<Question> AddQuestionAsync(AIReliefContext context, string category, string mainText)
    {
        var question = new Question
        {
            MainText = mainText,
            QuestionText = mainText,
            Option1 = "A",
            Option2 = "B",
            CorrectAnswer = "A",
            Category = category,
            ExplanationText = "Because A is correct."
        };

        context.Questions.Add(question);
        await context.SaveChangesAsync();
        return question;
    }
}
