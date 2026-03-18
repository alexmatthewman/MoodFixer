using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using AIRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Controllers
{
    public class HomeController : Controller
    {
        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(AIReliefContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
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

        [OutputCache(PolicyName = "AnonymousOnly")]
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var identityUser = await _userManager.GetUserAsync(User);
                if (identityUser != null)
                {
                    var appUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == identityUser.Email);

                    // Deny access if user's tenant doesn't match the current site (SystemAdmins are exempt)
                    if (appUser != null && appUser.AuthLevel != AuthLevel.SystemAdmin
                        && !string.Equals(appUser.TenantCode, GetCurrentTenantCode(), StringComparison.OrdinalIgnoreCase))
                    {
                        await _signInManager.SignOutAsync();
                        TempData["ErrorMessage"] = "Your account is not associated with this site.";
                        return View();
                    }

                    if (appUser?.AuthLevel == AuthLevel.User || appUser?.AuthLevel == AuthLevel.GroupAdmin)
                        return RedirectToAction("Index", "Lesson");
                }
            }
            return View();
        }

        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult About()
        {
            return View();
        }

        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult CausalReasoning()
        {
            return View();
        }

        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult CognitiveReflection()
        {
            return View();
        }

        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult Metacognition()
        {
            return View();
        }

        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult ReadingComprehension()
        {
            return View();
        }

        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult WorkingMemory()
        {
            return View();
        }

        [OutputCache(PolicyName = "PublicAlways")]
        public IActionResult ConfidenceCalibration()
        {
            return View();
        }


        [Authorize]
        public async Task<IActionResult> MyQuestions()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
                return Forbid();

            var appUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == identityUser.Email);
            if (appUser == null)
                return Forbid();

            if (appUser.AuthLevel == AuthLevel.SystemAdmin)
                return Forbid();

            var attempts = await _context.UserQuestions
                .Include(uq => uq.Question)
                .Where(uq => uq.UserID == appUser.ID)
                .OrderByDescending(uq => uq.DateLastAttempted ?? uq.DateFirstAttempted)
                .ToListAsync();

            return View(attempts);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        
    }
}
