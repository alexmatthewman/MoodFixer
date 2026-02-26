using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIRelief.Models;
using AIRelief.Services;
using System.Linq;
using System.Threading.Tasks;

namespace AIRelief.Controllers
{
    [Authorize]
    [Route("Admin/GroupAdmin")]
    public class GroupAdminController : Controller
    {
        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AdminAuthorizationService _authService;

        public GroupAdminController(AIReliefContext context, UserManager<IdentityUser> userManager, AdminAuthorizationService authService)
        {
            _context = context;
            _userManager = userManager;
            _authService = authService;
        }

        private async Task<User> GetCurrentUserAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            return await _authService.GetAppUserAsync(identityUser);
        }

        // ========== GROUP MANAGEMENT ==========

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            var users = await _context.Users
                .Where(u => u.GroupId == appUser.GroupId)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return View("~/Views/Admin/GroupAdmin/Index.cshtml", users);
        }

        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            if (id == null)
                return NotFound();

            var user = await _context.Users
                .Include(u => u.Group)
                .FirstOrDefaultAsync(u => u.ID == id && u.GroupId == appUser.GroupId);

            if (user == null)
                return NotFound();

            var userStats = await _context.UserStatistics
                .Where(s => s.UserId == user.ID)
                .ToListAsync();

            var viewModel = new { User = user, Statistics = userStats };
            return View("~/Views/Admin/GroupAdmin/Details.cshtml", viewModel);
        }

        [Route("ManageUsers/{id}")]
        public async Task<IActionResult> ManageUsers(int? id)
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            if (id == null || id != appUser.GroupId)
                return NotFound();

            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.ID == id);

            if (group == null)
                return NotFound();

            return View("~/Views/Admin/GroupAdmin/ManageUsers.cshtml", group);
        }
    }
}
