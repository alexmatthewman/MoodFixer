using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIRelief.Models;
using AIRelief.Services;
using System.Collections.Generic;
using System.IO;
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

            ViewBag.CurrentUserId = appUser.ID;
            ViewBag.GroupId = appUser.GroupId;
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

            ViewBag.Statistics = userStats;
            return View("~/Views/Admin/GroupAdmin/Details.cshtml", user);
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

        [Route("Groups/{groupId}/CreateUser")]
        [HttpGet]
        public async Task<IActionResult> CreateUser(int groupId)
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            if (appUser.GroupId != groupId)
                return Forbid();

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return NotFound();

            var user = new User { GroupId = groupId };
            ViewBag.Group = group;
            return View("~/Views/Admin/GroupAdmin/CreateUser.cshtml", user);
        }

        [Route("Groups/{groupId}/CreateUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(int groupId, User user, string password)
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            if (appUser.GroupId != groupId)
                return Forbid();

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "A password is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.Group = group;
                return View("~/Views/Admin/GroupAdmin/CreateUser.cshtml", user);
            }

            // Check license capacity
            var usedLicenses = await _context.Users.CountAsync(u => u.GroupId == groupId);
            if (usedLicenses >= group.NumberOfUserLicenses)
            {
                ModelState.AddModelError(string.Empty, "No available licenses remain for this group.");
                ViewBag.Group = group;
                return View("~/Views/Admin/GroupAdmin/CreateUser.cshtml", user);
            }

            // System Admins cannot be created through group admin
            if (user.AuthLevel == AuthLevel.SystemAdmin)
            {
                ModelState.AddModelError("AuthLevel", "System Admin accounts cannot be created here.");
                ViewBag.Group = group;
                return View("~/Views/Admin/GroupAdmin/CreateUser.cshtml", user);
            }

            // Check for duplicate email
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                ViewBag.Group = group;
                return View("~/Views/Admin/GroupAdmin/CreateUser.cshtml", user);
            }

            // Create the Identity account so the user can log in
            var identityUser = new IdentityUser { UserName = user.Email, Email = user.Email };
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                ViewBag.Group = group;
                return View("~/Views/Admin/GroupAdmin/CreateUser.cshtml", user);
            }

            user.GroupId = groupId;
            user.CreatedDate = System.DateTime.UtcNow;
            if (group.ExpiryDateTime.HasValue)
                user.ExpiryDate = group.ExpiryDateTime;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Route("Groups/{groupId}/BulkAddUsers")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAddUsers(int groupId, Microsoft.AspNetCore.Http.IFormFile csvFile)
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            if (appUser.GroupId != groupId)
                return Forbid();

            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.ID == groupId);
            if (group == null)
                return NotFound();

            if (csvFile == null || csvFile.Length == 0 || !csvFile.FileName.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["BulkError"] = "Please upload a valid CSV file.";
                return RedirectToAction(nameof(ManageUsers), new { id = groupId });
            }

            var results = new List<string>();
            int added = 0;

            using var reader = new StreamReader(csvFile.OpenReadStream());
            var headerLine = await reader.ReadLineAsync();
            if (headerLine == null)
            {
                TempData["BulkError"] = "The CSV file is empty.";
                return RedirectToAction(nameof(ManageUsers), new { id = groupId });
            }

            int rowIndex = 1;
            while (!reader.EndOfStream)
            {
                rowIndex++;
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var columns = line.Split(',');
                if (columns.Length < 3)
                {
                    results.Add($"Row {rowIndex}: Invalid format (expected Name, Email, AuthLevel).");
                    continue;
                }

                var name = columns[0].Trim().Trim('"');
                var email = columns[1].Trim().Trim('"');
                var authLevelStr = columns[2].Trim().Trim('"');

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                {
                    results.Add($"Row {rowIndex}: Name and Email are required.");
                    continue;
                }

                if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                {
                    results.Add($"Row {rowIndex}: '{email}' is not a valid email address.");
                    continue;
                }

                if (!System.Enum.TryParse<AuthLevel>(authLevelStr, true, out var authLevel))
                    authLevel = AuthLevel.User;

                if (authLevel == AuthLevel.SystemAdmin)
                {
                    results.Add($"Row {rowIndex}: System Admin accounts cannot be created via bulk import. '{name}' was skipped.");
                    continue;
                }

                var usedLicenses = group.Users.Count + added;
                if (usedLicenses >= group.NumberOfUserLicenses)
                {
                    results.Add($"Row {rowIndex}: No licenses remaining. '{name}' was not added.");
                    continue;
                }

                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    results.Add($"Row {rowIndex}: A user with email '{email}' already exists. Skipped.");
                    continue;
                }

                var password = columns.Length >= 4 ? columns[3].Trim().Trim('"') : null;
                if (string.IsNullOrWhiteSpace(password))
                    password = "TempPass123!";

                // Create the Identity account so the user can log in
                var identityUser = new IdentityUser { UserName = email, Email = email };
                var createResult = await _userManager.CreateAsync(identityUser, password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    results.Add($"Row {rowIndex}: Could not create account for '{name}' ({email}): {errors}");
                    continue;
                }

                var user = new User
                {
                    Name = name,
                    Email = email,
                    AuthLevel = authLevel,
                    GroupId = groupId,
                    CreatedDate = System.DateTime.UtcNow,
                    ExpiryDate = group.ExpiryDateTime
                };

                _context.Users.Add(user);
                added++;
                results.Add($"Row {rowIndex}: '{name}' ({email}) added successfully.");
            }

            if (added > 0)
                await _context.SaveChangesAsync();

            TempData["BulkResults"] = System.Text.Json.JsonSerializer.Serialize(results);
            TempData["BulkAdded"] = added;
            return RedirectToAction(nameof(ManageUsers), new { id = groupId });
        }

        [Route("Groups/{groupId}/BulkUsersTemplate")]
        [HttpGet]
        public async Task<IActionResult> BulkUsersTemplate(int groupId)
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            if (appUser.GroupId != groupId)
                return Forbid();

            var csv = "Name,Email,AuthLevel,Password\r\nJane Smith,jane.smith@example.com,User,TempPass123!\r\nJohn Doe,john.doe@example.com,GroupAdmin,TempPass123!\r\n";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "bulk_users_template.csv");
        }

        [Route("GroupSettings")]
        public async Task<IActionResult> GroupSettings()
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.ID == appUser.GroupId);

            if (group == null)
                return NotFound();

            return View("~/Views/Admin/GroupAdmin/GroupSettings.cshtml", group);
        }

        [Route("GroupStatistics")]
        public async Task<IActionResult> GroupStatistics()
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            var users = await _context.Users
                .Where(u => u.GroupId == appUser.GroupId)
                .Include(u => u.Statistics)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return View("~/Views/Admin/GroupAdmin/GroupStatistics.cshtml", users);
        }

        [Route("Users/Remove/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUser(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (!await _authService.IsValidGroupAdminAsync(appUser))
                return Forbid();

            if (appUser.ID == id)
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.GroupId != appUser.GroupId)
                return NotFound();

            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{user.Name} has been removed from the group.";
            return RedirectToAction(nameof(Index));
        }
    }
}
