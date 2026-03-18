using System.Text;
using AIRelief.Controllers;
using AIRelief.Models;
using AIRelief.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Tests;

public class SystemAdminControllerTests
{
    [Fact]
    public async Task CreateGroup_CreatesTestGroup()
    {
        await using var fixture = new ControllerTestFixture();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        fixture.SetCurrentUser(admin);

        var controller = fixture.InitializeController(new SystemAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService, fixture.TenantRegistry, fixture.EmailService, fixture.OutputCacheStore, fixture.SystemAdminLogger));
        var group = new Group
        {
            Name = "TestGroup",
            NumberOfUserLicenses = 10,
            QueryFrequency = QueryFrequency.Weekly,
            QueryTimeToCompleteDays = 1,
            QueryPassingGrade = 50,
            GroupImageUrl = "https://example.com/logo.png",
            QueryQuestionsFocussed = true
        };

        controller.TryValidateModel(group);

        var result = await controller.CreateGroup(group);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(SystemAdminController.Groups), redirect.ActionName);
        var saved = await fixture.Context.Groups.SingleAsync(g => g.Name == "TestGroup");
        Assert.True(saved.CreatedDate <= DateTime.UtcNow);
        Assert.False(saved.QueryQuestionsRandom);
    }

    [Fact]
    public async Task AddUser_CreatesGroupUser_AndRemoveUserDeletesIt()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        fixture.SetCurrentUser(admin);

        var controller = fixture.InitializeController(new SystemAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService, fixture.TenantRegistry, fixture.EmailService, fixture.OutputCacheStore, fixture.SystemAdminLogger));
        var user = new User
        {
            Name = "TestUser2",
            Email = $"testuser2-{Guid.NewGuid():N}@testgroup.local",
            AuthLevel = AuthLevel.User,
            GroupId = group.ID
        };

        controller.TryValidateModel(user);

        var addResult = await controller.AddUser(group.ID, user, "TempPass123!");

        var addRedirect = Assert.IsType<RedirectToActionResult>(addResult);
        Assert.Equal(nameof(SystemAdminController.GroupDetails), addRedirect.ActionName);

        var saved = await fixture.Context.Users.SingleAsync(u => u.Email == user.Email);
        Assert.Equal(group.ID, saved.GroupId);
        Assert.True(fixture.IdentityUserExists(user.Email));

        var removeResult = await controller.RemoveUser(saved.ID);

        var removeRedirect = Assert.IsType<RedirectToActionResult>(removeResult);
        Assert.Equal(nameof(SystemAdminController.Users), removeRedirect.ActionName);
        Assert.False(await fixture.Context.Users.AnyAsync(u => u.Email == user.Email));
        Assert.False(fixture.IdentityUserExists(user.Email));
    }

    [Fact]
    public async Task BulkAddUsers_AddsOnlyValidRows()
    {
        await using var fixture = new ControllerTestFixture();
        var group = await fixture.CreateTestGroupAsync(licenses: 4);
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        await fixture.CreateAppUserAsync("TestUserExisting", "existing@testgroup.local", AuthLevel.User, group.ID);
        fixture.SetCurrentUser(admin);

        var controller = fixture.InitializeController(new SystemAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService, fixture.TenantRegistry, fixture.EmailService, fixture.OutputCacheStore, fixture.SystemAdminLogger));

        const string csv = "Name,Email,AuthLevel,Password\r\n"
            + "TestUser2,testuser2@testgroup.local,User,TempPass123!\r\n"
            + "TestUser3,testuser3@testgroup.local,SystemAdmin,TempPass123!\r\n"
            + "TestUser4,existing@testgroup.local,User,TempPass123!\r\n";

        var bytes = Encoding.UTF8.GetBytes(csv);
        using var stream = new MemoryStream(bytes);
        var csvFile = new FormFile(stream, 0, bytes.Length, "csvFile", "users.csv");

        var result = await controller.BulkAddUsers(group.ID, csvFile);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(SystemAdminController.GroupDetails), redirect.ActionName);
        Assert.True(await fixture.Context.Users.AnyAsync(u => u.Email == "testuser2@testgroup.local"));
        Assert.False(await fixture.Context.Users.AnyAsync(u => u.Email == "testuser3@testgroup.local"));
        Assert.Equal(1, Assert.IsType<int>(controller.TempData["BulkAdded"]));
    }

    [Fact]
    public async Task AddSystemAdmin_CreatesSystemAdminUser()
    {
        await using var fixture = new ControllerTestFixture();
        var admin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        fixture.SetCurrentUser(admin);

        var controller = fixture.InitializeController(new SystemAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService, fixture.TenantRegistry, fixture.EmailService, fixture.OutputCacheStore, fixture.SystemAdminLogger));

        var result = await controller.AddSystemAdmin("TestUser2", "testuser2@system.local", "TempPass123!");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(SystemAdminController.ManageSystemAdmins), redirect.ActionName);
        var saved = await fixture.Context.Users.SingleAsync(u => u.Email == "testuser2@system.local");
        Assert.Equal(AuthLevel.SystemAdmin, saved.AuthLevel);
        Assert.True(fixture.IdentityUserExists("testuser2@system.local"));
    }

    [Fact]
    public async Task RemoveSystemAdmin_DemotesOtherAdminToUser()
    {
        await using var fixture = new ControllerTestFixture();
        var currentAdmin = await fixture.CreateAppUserAsync("TestUser1", "testuser1@system.local", AuthLevel.SystemAdmin);
        var targetAdmin = await fixture.CreateAppUserAsync("TestUser2", "testuser2@system.local", AuthLevel.SystemAdmin);
        fixture.SetCurrentUser(currentAdmin);

        var controller = fixture.InitializeController(new SystemAdminController(fixture.Context, fixture.UserManager, fixture.AuthorizationService, fixture.TenantRegistry, fixture.EmailService, fixture.OutputCacheStore, fixture.SystemAdminLogger));

        var result = await controller.RemoveSystemAdmin(targetAdmin.ID);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(SystemAdminController.ManageSystemAdmins), redirect.ActionName);
        var updated = await fixture.Context.Users.SingleAsync(u => u.ID == targetAdmin.ID);
        Assert.Equal(AuthLevel.User, updated.AuthLevel);
        Assert.NotNull(updated.LastModifiedDate);
    }
}