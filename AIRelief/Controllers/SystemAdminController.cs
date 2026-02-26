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
    [Route("Admin/SystemAdmin")]
    public class SystemAdminController : Controller
    {
        private readonly AIReliefContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AdminAuthorizationService _authService;

        public SystemAdminController(AIReliefContext context, UserManager<IdentityUser> userManager, AdminAuthorizationService authService)
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

        // ========== GROUPS ==========

        [Route("Groups")]
        public async Task<IActionResult> Groups()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var groups = await _context.Groups.ToListAsync();
            return View("~/Views/Admin/SystemAdmin/Groups.cshtml", groups);
        }

        [Route("Groups/Create")]
        [HttpGet]
        public async Task<IActionResult> CreateGroup()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            return View("~/Views/Admin/SystemAdmin/CreateGroup.cshtml", new Group());
        }

        [Route("Groups/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(Group group)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/SystemAdmin/CreateGroup.cshtml", group);

            // Ensure only one of the question options is selected: focussed is default
            // The UI will submit QueryQuestionsFocussed as true/false; set Random to the inverse
            group.QueryQuestionsRandom = !group.QueryQuestionsFocussed;

            group.CreatedDate = System.DateTime.UtcNow;
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Groups));
        }

        [Route("Groups/Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditGroup(int? id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (id == null)
                return NotFound();

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                return NotFound();

            return View("~/Views/Admin/SystemAdmin/EditGroup.cshtml", group);
        }

        [Route("Groups/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroup(int id, Group group)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (id != group.ID)
                return NotFound();

            if (!ModelState.IsValid)
                return View("~/Views/Admin/SystemAdmin/EditGroup.cshtml", group);

            group.LastModifiedDate = System.DateTime.UtcNow;

            // Keep QueryQuestionsRandom consistent with focussed selection
            group.QueryQuestionsRandom = !group.QueryQuestionsFocussed;
            _context.Attach(group).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Groups.Any(e => e.ID == group.ID))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Groups));
        }

        [Route("Groups/Details/{id}")]
        public async Task<IActionResult> GroupDetails(int? id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (id == null)
                return NotFound();

            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (group == null)
                return NotFound();

            return View("~/Views/Admin/SystemAdmin/GroupDetails.cshtml", group);
        }

        [Route("Groups/Delete/{id}")]
        [HttpGet]
        public async Task<IActionResult> DeleteGroup(int? id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (id == null)
                return NotFound();

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                return NotFound();

            return View("~/Views/Admin/SystemAdmin/DeleteGroup.cshtml", group);
        }

        [Route("Groups/Delete/{id}")]
        [HttpPost]
        [ActionName("DeleteGroup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroupConfirmed(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var group = await _context.Groups.FindAsync(id);
            if (group != null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Groups));
        }

        // ========== USERS ==========

        [Route("Users")]
        public async Task<IActionResult> Users()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var users = await _context.Users
                .Include(u => u.Group)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return View("~/Views/Admin/SystemAdmin/Users.cshtml", users);
        }
    }
}
