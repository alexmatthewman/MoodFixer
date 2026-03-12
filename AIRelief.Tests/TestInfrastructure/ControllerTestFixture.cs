using System.Security.Claims;
using AIRelief.Models;
using AIRelief.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AIRelief.Tests.TestInfrastructure;

internal sealed class ControllerTestFixture : IAsyncDisposable
{
    private readonly string _databaseName = $"AIReliefTests_{Guid.NewGuid():N}";
    private readonly Dictionary<string, IdentityUser> _identityStore = new(StringComparer.OrdinalIgnoreCase);
    private readonly ServiceProvider _serviceProvider;
    private IdentityUser? _currentIdentityUser;

    public ControllerTestFixture()
    {
        Options = new DbContextOptionsBuilder<AIReliefContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        Context = new AIReliefContext(Options);
        _serviceProvider = BuildServiceProvider();
        UserManager = CreateUserManagerMock(_identityStore, () => _currentIdentityUser).Object;
        SignInManager = CreateSignInManagerMock(UserManager).Object;
        AuthorizationService = new AdminAuthorizationService(Context, UserManager);
        TenantRegistry = new TenantRegistry(new Dictionary<string, TenantConfig>
        {
            ["relief"] = new TenantConfig
            {
                MarketCode = "relief",
                DefaultLanguage = "en",
                SupportedLanguages = ["en"],
                SiteName = "AI Relief",
                ThemeFolder = "relief"
            }
        });
    }

    public AIReliefContext Context { get; }

    public DbContextOptions<AIReliefContext> Options { get; }

    public UserManager<IdentityUser> UserManager { get; }

    public SignInManager<IdentityUser> SignInManager { get; }

    public AdminAuthorizationService AuthorizationService { get; }

    public TenantRegistry TenantRegistry { get; }

    public IEmailService EmailService { get; } = Mock.Of<IEmailService>();

    public IOutputCacheStore OutputCacheStore { get; } = Mock.Of<IOutputCacheStore>();

    public Group? TestGroup { get; private set; }

    public User? CurrentAppUser { get; private set; }

    public async Task<Group> CreateTestGroupAsync(
        string name = "TestGroup",
        int licenses = 10,
        QueryFrequency queryFrequency = QueryFrequency.Weekly)
    {
        var group = new Group
        {
            Name = name,
            NumberOfUserLicenses = licenses,
            QueryFrequency = queryFrequency,
            QueryTimeToCompleteDays = 1,
            QueryPassingGrade = 50,
            GroupImageUrl = "https://example.com/logo.png",
            QueryQuestionsFocussed = true,
            QueryQuestionsRandom = false,
            TenantCode = "relief"
        };

        Context.Groups.Add(group);
        await Context.SaveChangesAsync();
        TestGroup = group;
        return group;
    }

    public async Task<User> CreateAppUserAsync(
        string name,
        string email,
        AuthLevel authLevel,
        int? groupId = null,
        QueryFrequency? queryFrequency = null,
        bool addIdentityUser = true)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            AuthLevel = authLevel,
            GroupId = groupId,
            QueryFrequency = queryFrequency,
            TenantCode = "relief",
            CreatedDate = DateTime.UtcNow
        };

        Context.Users.Add(user);
        await Context.SaveChangesAsync();

        if (addIdentityUser)
        {
            var identityUser = new IdentityUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = email,
                UserName = email
            };
            _identityStore[email] = identityUser;
        }

        return user;
    }

    public void SetCurrentUser(User appUser)
    {
        CurrentAppUser = appUser;
        _currentIdentityUser = _identityStore[appUser.Email];
    }

    public bool IdentityUserExists(string email) => _identityStore.ContainsKey(email);

    public TController InitializeController<TController>(TController controller)
        where TController : Controller
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProvider,
            Session = new TestSession()
        };

        httpContext.Items["Tenant"] = new TenantConfig
        {
            MarketCode = "relief",
            DefaultLanguage = "en",
            SupportedLanguages = ["en"],
            SiteName = "AI Relief",
            ThemeFolder = "relief"
        };

        var claims = new List<Claim>();
        if (_currentIdentityUser?.Email is { Length: > 0 } email)
        {
            claims.Add(new Claim(ClaimTypes.Name, email));
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
            RouteData = new RouteData()
        };
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        controller.Url = CreateUrlHelperMock().Object;
        controller.ObjectValidator = _serviceProvider.GetRequiredService<IObjectModelValidator>();

        return controller;
    }

    public async ValueTask DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.AddControllers();
        return services.BuildServiceProvider();
    }

    private static Mock<IUrlHelper> CreateUrlHelperMock()
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(m => m.Action(It.IsAny<UrlActionContext>())).Returns("/");
        urlHelper.Setup(m => m.Link(It.IsAny<string>(), It.IsAny<object>())).Returns("/");
        return urlHelper;
    }

    private static Mock<UserManager<IdentityUser>> CreateUserManagerMock(
        IDictionary<string, IdentityUser> identityStore,
        Func<IdentityUser?> currentUserAccessor)
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        var userManager = new Mock<UserManager<IdentityUser>>(
            store.Object,
            null!,
            new PasswordHasher<IdentityUser>(),
            Array.Empty<IUserValidator<IdentityUser>>(),
            Array.Empty<IPasswordValidator<IdentityUser>>(),
            null!,
            null!,
            null!,
            null!);

        userManager
            .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(() => currentUserAccessor());

        userManager
            .Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
            .ReturnsAsync((IdentityUser user, string password) =>
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Password is required." });
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Email is required." });
                }

                if (identityStore.ContainsKey(user.Email))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Duplicate user." });
                }

                user.Id ??= Guid.NewGuid().ToString();
                user.UserName ??= user.Email;
                identityStore[user.Email] = user;
                return IdentityResult.Success;
            });

        userManager
            .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) =>
                identityStore.TryGetValue(email, out var user) ? user : null);

        userManager
            .Setup(m => m.DeleteAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync((IdentityUser user) =>
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    identityStore.Remove(user.Email);
                }

                return IdentityResult.Success;
            });

        return userManager;
    }

    private static Mock<SignInManager<IdentityUser>> CreateSignInManagerMock(
        UserManager<IdentityUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();
        var signInManager = new Mock<SignInManager<IdentityUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            null!,
            null!,
            null!,
            null!);

        signInManager
            .Setup(m => m.SignOutAsync())
            .Returns(Task.CompletedTask);

        return signInManager;
    }
}