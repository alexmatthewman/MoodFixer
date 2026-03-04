using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AIRelief.Models;
using Microsoft.EntityFrameworkCore;

namespace AIRelief.Controllers
{
    public class HomeController : Controller
    {
        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(AIReliefContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {           
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult CausalReasoning()
        {
            return View();
        }

        public IActionResult CognitiveReflection()
        {
            return View();
        }

        public IActionResult Metacognition()
        {
            return View();
        }

        public IActionResult ReadingComprehension()
        {
            return View();
        }

        public IActionResult ShortTermMemory()
        {
            return View();
        }

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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
