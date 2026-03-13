using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using AIRelief.Models;

namespace AIRelief.Controllers
{
    public class SignupController : Controller
    {
        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public SignupController(AIReliefContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        private string GetCurrentTenantCode()
        {
            var tenant = HttpContext.Items["Tenant"] as TenantConfig;
            return tenant?.MarketCode ?? "relief";
        }

        [HttpGet]
        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Payment(string planType, string frequency, int months, int groupSize, string price)
        {
            var model = new PaymentViewModel
            {
                PlanType = planType ?? "individual",
                Frequency = frequency ?? "weekly",
                Months = months > 0 ? months : 3,
                GroupSize = groupSize > 0 ? groupSize : 1,
                DisplayPrice = price ?? "$0"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterAndPay(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Payment", model);

            // Check for duplicate email
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                return View("Payment", model);
            }

            // Create the Identity account
            var identityUser = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View("Payment", model);
            }

            var tenantCode = GetCurrentTenantCode();
            var isGroup = string.Equals(model.PlanType, "group", StringComparison.OrdinalIgnoreCase)
                          && model.GroupSize > 1;

            var frequency = model.Frequency?.ToLowerInvariant() == "monthly"
                ? QueryFrequency.Monthly
                : QueryFrequency.Weekly;

            var expiryDate = DateTime.UtcNow.AddMonths(model.Months);

            if (isGroup)
            {
                // Create the group
                var group = new Group
                {
                    Name = $"{model.FullName}'s Group",
                    NumberOfUserLicenses = model.GroupSize,
                    QueryFrequency = frequency,
                    ExpiryDateTime = expiryDate,
                    ExpiryDays = model.Months * 30,
                    TenantCode = tenantCode,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Groups.Add(group);
                await _context.SaveChangesAsync();

                // Create the GroupAdmin user
                var appUser = new User
                {
                    Email = model.Email,
                    Name = model.FullName,
                    AuthLevel = AuthLevel.GroupAdmin,
                    GroupId = group.ID,
                    ExpiryDate = expiryDate,
                    QueryFrequency = frequency,
                    TenantCode = tenantCode,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Users.Add(appUser);
                await _context.SaveChangesAsync();

                // Sign in and redirect with welcome flag
                await _signInManager.SignInAsync(identityUser, isPersistent: false);
                TempData["ShowGroupAdminWelcome"] = true;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Create individual user
                var appUser = new User
                {
                    Email = model.Email,
                    Name = model.FullName,
                    AuthLevel = AuthLevel.User,
                    ExpiryDate = expiryDate,
                    QueryFrequency = frequency,
                    TenantCode = tenantCode,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Users.Add(appUser);
                await _context.SaveChangesAsync();

                // Sign in and redirect to lessons
                await _signInManager.SignInAsync(identityUser, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult ContactSales()
        {
            return View("~/Views/Contact/Index.cshtml");
        }
    }

    public class PaymentViewModel
    {
        // Plan info (passed from signup page)
        public string PlanType { get; set; } = "individual";
        public string Frequency { get; set; } = "weekly";
        public int Months { get; set; } = 3;
        public int GroupSize { get; set; } = 1;
        public string DisplayPrice { get; set; } = "$0";

        // Registration fields
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(400, ErrorMessage = "Name cannot exceed 400 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
