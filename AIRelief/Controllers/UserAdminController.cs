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
    [Route("Admin/UserAdmin")]
    public class UserAdminController : Controller
    {
        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AdminAuthorizationService _authService;

        public UserAdminController(AIReliefContext context, UserManager<IdentityUser> userManager, AdminAuthorizationService authService)
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

        // ========== USER ADMIN ==========

        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var users = await _context.Users
                .Include(u => u.Group)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return View("~/Views/Admin/UserAdmin/Index.cshtml", users);
        }
    }
}
