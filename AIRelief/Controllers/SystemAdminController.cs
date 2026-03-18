using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using AIRelief.Models;
using AIRelief.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly TenantRegistry _tenantRegistry;
        private readonly IEmailService _emailService;
        private readonly IOutputCacheStore _outputCacheStore;

        public SystemAdminController(AIReliefContext context, UserManager<IdentityUser> userManager, AdminAuthorizationService authService, TenantRegistry tenantRegistry, IEmailService emailService, IOutputCacheStore outputCacheStore)
        {
            _context = context;
            _userManager = userManager;
            _authService = authService;
            _tenantRegistry = tenantRegistry;
            _emailService = emailService;
            _outputCacheStore = outputCacheStore;
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

            ViewBag.Tenants = _tenantRegistry.All.ToList();
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
            {
                ViewBag.Tenants = _tenantRegistry.All.ToList();
                return View("~/Views/Admin/SystemAdmin/CreateGroup.cshtml", group);
            }

            // Ensure only one of the question options is selected: focussed is default
            // The UI will submit QueryQuestionsFocussed as true/false; set Random to the inverse
            group.QueryQuestionsRandom = !group.QueryQuestionsFocussed;

            group.CreatedDate = System.DateTime.UtcNow;
            group.ExpiryDateTime = group.ExpiryDays.HasValue
                ? group.CreatedDate.AddDays(group.ExpiryDays.Value)
                : (System.DateTime?)null;
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

            ViewBag.Tenants = _tenantRegistry.All.ToList();
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
            {
                ViewBag.Tenants = _tenantRegistry.All.ToList();
                return View("~/Views/Admin/SystemAdmin/EditGroup.cshtml", group);
            }

            group.LastModifiedDate = System.DateTime.UtcNow;

            // Keep QueryQuestionsRandom consistent with focussed selection
            group.QueryQuestionsRandom = !group.QueryQuestionsFocussed;

            group.ExpiryDateTime = group.ExpiryDays.HasValue
                ? System.DateTime.UtcNow.AddDays(group.ExpiryDays.Value)
                : (System.DateTime?)null;

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

        [Route("Groups/Statistics/{id}")]
        public async Task<IActionResult> GroupStatistics(int? id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (id == null)
                return NotFound();

            var group = await _context.Groups.FindAsync(id);
            if (group == null)
                return NotFound();

            var users = await _context.Users
                .Where(u => u.GroupId == id)
                .Include(u => u.Statistics)
                .OrderBy(u => u.Name)
                .ToListAsync();

            var vm = BuildGroupStatisticsViewModel(group.ID, group.Name, users);
            return View("~/Views/Admin/GroupAdmin/GroupStatistics.cshtml", vm);
        }

        private static GroupStatisticsViewModel BuildGroupStatisticsViewModel(int groupId, string groupName, List<User> users)
        {
            var members = users.Select(u =>
            {
                var s = u.Statistics;
                if (s == null)
                    return new GroupMemberStatRow { Name = u.Name, Email = u.Email };

                int totalAttempts =
                    s.CausalReasoningAttempts + s.CognitiveReflectionAttempts +
                    s.ConfidenceCalibrationAttempts + s.MetacognitionAttempts +
                    s.ReadingComprehensionAttempts + s.ShortTermMemoryAttempts;

                return new GroupMemberStatRow
                {
                    Name              = u.Name,
                    Email             = u.Email,
                    LessonsCompleted  = (int)Math.Round(totalAttempts / 6.0),
                    OverallPercent    = totalAttempts > 0
                                            ? (int)Math.Round((double)s.OverallWeightedAverage * 100)
                                            : null,
                    CausalReasoning       = s.CausalReasoningAttempts       > 0 ? (int)Math.Round((double)s.CausalReasoningWeightedAverage       * 100) : null,
                    CognitiveReflection   = s.CognitiveReflectionAttempts   > 0 ? (int)Math.Round((double)s.CognitiveReflectionWeightedAverage   * 100) : null,
                    ConfidenceCalibration = s.ConfidenceCalibrationAttempts > 0 ? (int)Math.Round((double)s.ConfidenceCalibrationWeightedAverage * 100) : null,
                    Metacognition         = s.MetacognitionAttempts         > 0 ? (int)Math.Round((double)s.MetacognitionWeightedAverage         * 100) : null,
                    ReadingComprehension  = s.ReadingComprehensionAttempts  > 0 ? (int)Math.Round((double)s.ReadingComprehensionWeightedAverage  * 100) : null,
                    ShortTermMemory       = s.ShortTermMemoryAttempts       > 0 ? (int)Math.Round((double)s.ShortTermMemoryWeightedAverage       * 100) : null,
                };
            }).ToList();

            return new GroupStatisticsViewModel
            {
                GroupId   = groupId,
                GroupName = groupName,
                Members   = members
            };
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

        // ========== ADD / BULK ADD USERS TO GROUP ==========

        [Route("Groups/{groupId}/AddUser")]
        [HttpGet]
        public async Task<IActionResult> AddUser(int groupId)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return NotFound();

            var user = new User { GroupId = groupId };
            ViewBag.Group = group;
            return View("~/Views/Admin/SystemAdmin/AddUser.cshtml", user);
        }

        [Route("Groups/{groupId}/AddUser")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUser(int groupId, User user, string password)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "A password is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.Group = group;
                return View("~/Views/Admin/SystemAdmin/AddUser.cshtml", user);
            }

            // System Admins cannot be created through group user add
            if (user.AuthLevel == AuthLevel.SystemAdmin)
            {
                ModelState.AddModelError("AuthLevel", "System Admin accounts cannot be created here.");
                ViewBag.Group = group;
                return View("~/Views/Admin/SystemAdmin/AddUser.cshtml", user);
            }

            // Check license capacity
            var usedLicenses = await _context.Users.CountAsync(u => u.GroupId == groupId);
            if (usedLicenses >= group.NumberOfUserLicenses)
            {
                ModelState.AddModelError(string.Empty, "No available licenses remain for this group.");
                ViewBag.Group = group;
                return View("~/Views/Admin/SystemAdmin/AddUser.cshtml", user);
            }

            // Check for duplicate email in app users
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "A user with this email already exists.");
                ViewBag.Group = group;
                return View("~/Views/Admin/SystemAdmin/AddUser.cshtml", user);
            }

            // Create the Identity account so the user can log in
            var identityUser = new IdentityUser { UserName = user.Email, Email = user.Email };
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                ViewBag.Group = group;
                return View("~/Views/Admin/SystemAdmin/AddUser.cshtml", user);
            }

            user.GroupId = groupId;
            user.TenantCode = group.TenantCode;
            user.CreatedDate = System.DateTime.UtcNow;
            if (group.ExpiryDateTime.HasValue)
                user.ExpiryDate = group.ExpiryDateTime;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(GroupDetails), new { id = groupId });
        }

        [Route("Groups/{groupId}/BulkAddUsers")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAddUsers(int groupId, Microsoft.AspNetCore.Http.IFormFile csvFile)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var group = await _context.Groups
                .Include(g => g.Users)
                .FirstOrDefaultAsync(g => g.ID == groupId);
            if (group == null)
                return NotFound();

            if (csvFile == null || csvFile.Length == 0 || !csvFile.FileName.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
            {
                TempData["BulkError"] = "Please upload a valid CSV file.";
                return RedirectToAction(nameof(GroupDetails), new { id = groupId });
            }

            var results = new List<string>();
            int added = 0;

            using var reader = new StreamReader(csvFile.OpenReadStream());
            var headerLine = await reader.ReadLineAsync();
            if (headerLine == null)
            {
                TempData["BulkError"] = "The CSV file is empty.";
                return RedirectToAction(nameof(GroupDetails), new { id = groupId });
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
                    TenantCode = group.TenantCode,
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
            return RedirectToAction(nameof(GroupDetails), new { id = groupId });
        }

        [Route("Groups/{groupId}/BulkUsersTemplate")]
        [HttpGet]
        public async Task<IActionResult> BulkUsersTemplate(int groupId)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var csv = "Name,Email,AuthLevel,Password\r\nJane Smith,jane.smith@example.com,User,TempPass123!\r\nJohn Doe,john.doe@example.com,GroupAdmin,TempPass123!\r\n";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "bulk_users_template.csv");
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

            ViewBag.CurrentUserId = appUser.ID;
            return View("~/Views/Admin/SystemAdmin/Users.cshtml", users);
        }

        [Route("Users/Remove/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUser(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (appUser.ID == id)
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{user.Name} has been removed.";
            return RedirectToAction(nameof(Users));
        }

        [Route("Users/Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var user = await _context.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.ID == id);
            if (user == null)
                return NotFound();

            return View("~/Views/Admin/SystemAdmin/EditUser.cshtml", user);
        }

        [Route("Users/Edit/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, QueryFrequency? queryFrequency)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var user = await _context.Users.Include(u => u.Group).FirstOrDefaultAsync(u => u.ID == id);
            if (user == null)
                return NotFound();

            // Group members inherit QueryFrequency from their group; only standalone users have it set directly
            if (user.GroupId == null)
            {
                if (queryFrequency == null)
                {
                    ModelState.AddModelError("QueryFrequency", "Query Frequency is required for users not in a group.");
                    return View("~/Views/Admin/SystemAdmin/EditUser.cshtml", user);
                }
                user.QueryFrequency = queryFrequency;
            }

            user.LastModifiedDate = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{user.Name}'s settings have been updated.";
            return RedirectToAction(nameof(Users));
        }

        [Route("Users/{id}/Questions")]
        public async Task<IActionResult> UserQuestions(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == id);
            if (user == null)
                return NotFound();

            var attempts = await _context.UserQuestions
                .Include(uq => uq.Question)
                .Where(uq => uq.UserID == id)
                .OrderByDescending(uq => uq.DateLastAttempted ?? uq.DateFirstAttempted)
                .ToListAsync();

            ViewBag.TargetUser = user;
            return View("~/Views/Admin/SystemAdmin/UserQuestions.cshtml", attempts);
        }

        // ========== SYSTEM ADMINS ==========

        [Route("SystemAdmins")]
        public async Task<IActionResult> ManageSystemAdmins()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var admins = await _context.Users
                .Where(u => u.AuthLevel == AuthLevel.SystemAdmin)
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.CurrentUserId = appUser.ID;
            return View("~/Views/Admin/SystemAdmin/ManageSystemAdmins.cshtml", admins);
        }

        [Route("SystemAdmins/Add")]
        [HttpGet]
        public async Task<IActionResult> AddSystemAdmin()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            return View("~/Views/Admin/SystemAdmin/AddSystemAdmin.cshtml");
        }

        [Route("SystemAdmins/Add")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSystemAdmin(string name, string email, string password)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Name, email and password are all required.";
                return View("~/Views/Admin/SystemAdmin/AddSystemAdmin.cshtml");
            }

            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.ErrorMessage = "A user with this email already exists.";
                return View("~/Views/Admin/SystemAdmin/AddSystemAdmin.cshtml");
            }

            var identityUser = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return View("~/Views/Admin/SystemAdmin/AddSystemAdmin.cshtml");
            }

            var newAdmin = new User
            {
                Name = name,
                Email = email,
                AuthLevel = AuthLevel.SystemAdmin,
                CreatedDate = System.DateTime.UtcNow
            };

            _context.Users.Add(newAdmin);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{name} has been added as a System Admin.";
            return RedirectToAction(nameof(ManageSystemAdmins));
        }

        [Route("SystemAdmins/Remove/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSystemAdmin(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (appUser.ID == id)
            {
                TempData["ErrorMessage"] = "You cannot remove yourself as a System Admin.";
                return RedirectToAction(nameof(ManageSystemAdmins));
            }

            var admin = await _context.Users.FindAsync(id);
            if (admin == null || admin.AuthLevel != AuthLevel.SystemAdmin)
                return NotFound();

            admin.AuthLevel = AuthLevel.User;
            admin.LastModifiedDate = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{admin.Name} has been removed as a System Admin.";
            return RedirectToAction(nameof(ManageSystemAdmins));
        }

        // ========== BULK QUESTIONS ==========

        private record QuestionDto(
            string? Category,
            string? QuestionText,
            string MainText,
            string? Image,
            string Option1,
            string Option2,
            string? Option3,
            string? Option4,
            string? Option5,
            string CorrectAnswer,
            string? BestAnswersRaw,
            string? ExplanationText,
            string? ExplanationImage
        );

        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = null
        };

        private static QuestionDto QuestionToDto(Question q) =>
            new(q.Category, q.QuestionText, q.MainText, q.Image,
                q.Option1, q.Option2, q.Option3, q.Option4, q.Option5,
                q.CorrectAnswer, q.BestAnswersRaw,
                q.ExplanationText, q.ExplanationImage);

        private static byte[] QuestionsToJsonBytes(IEnumerable<Question> questions)
        {
            var dtos = questions.Select(QuestionToDto).ToList();
            return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(dtos, JsonOptions);
        }

        private static byte[] QuestionsToCsvBytes(IEnumerable<Question> questions)
        {
            static string CsvField(string? value)
            {
                if (string.IsNullOrEmpty(value)) return string.Empty;
                // Wrap in quotes if the value contains a comma, double-quote, or newline
                if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                    return '"' + value.Replace("\"", "\"\"") + '"';
                return value;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Category,QuestionText,MainText,Image,Option1,Option2,Option3,Option4,Option5,CorrectAnswer,BestAnswersRaw,ExplanationText,ExplanationImage");
            foreach (var q in questions)
            {
                var dto = QuestionToDto(q);
                sb.AppendLine(string.Join(",",
                    CsvField(dto.Category), CsvField(dto.QuestionText), CsvField(dto.MainText),
                    CsvField(dto.Image), CsvField(dto.Option1), CsvField(dto.Option2),
                    CsvField(dto.Option3), CsvField(dto.Option4), CsvField(dto.Option5),
                    CsvField(dto.CorrectAnswer), CsvField(dto.BestAnswersRaw),
                    CsvField(dto.ExplanationText), CsvField(dto.ExplanationImage)));
            }
            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }

        [Route("BulkQuestions")]
        public async Task<IActionResult> BulkQuestions()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            return View("~/Views/Admin/SystemAdmin/BulkQuestions.cshtml");
        }

        [Route("BulkQuestions/Download")]
        public async Task<IActionResult> BulkQuestionsDownload()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var questions = await _context.Questions.OrderBy(q => q.Category).ThenBy(q => q.ID).ToListAsync();
            var bytes = QuestionsToJsonBytes(questions);
            return File(bytes, "application/json", $"questions_{System.DateTime.UtcNow:yyyyMMdd}.json");
        }

        [Route("BulkQuestions/DownloadCsv")]
        public async Task<IActionResult> BulkQuestionsDownloadCsv()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var questions = await _context.Questions.OrderBy(q => q.Category).ThenBy(q => q.ID).ToListAsync();
            var bytes = QuestionsToCsvBytes(questions);
            return File(bytes, "text/csv", $"questions_{System.DateTime.UtcNow:yyyyMMdd}.csv");
        }

        [Route("BulkQuestions/Upload")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkQuestionsUpload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a JSON file to upload.";
                return RedirectToAction(nameof(BulkQuestions));
            }

            List<QuestionDto>? dtos;
            try
            {
                dtos = await System.Text.Json.JsonSerializer.DeserializeAsync<List<QuestionDto>>(
                    file.OpenReadStream(), JsonOptions);
            }
            catch (System.Text.Json.JsonException ex)
            {
                TempData["ErrorMessage"] = $"Invalid JSON file: {ex.Message}";
                return RedirectToAction(nameof(BulkQuestions));
            }

            if (dtos == null || dtos.Count == 0)
            {
                TempData["ErrorMessage"] = "The uploaded file is empty or contains no questions.";
                return RedirectToAction(nameof(BulkQuestions));
            }

            var added = 0;
            var skipped = 0;
            var errors = new List<string>();

            for (int i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];
                var itemLabel = $"Item {i + 1}";

                if (string.IsNullOrWhiteSpace(dto.MainText) || string.IsNullOrWhiteSpace(dto.Option1)
                    || string.IsNullOrWhiteSpace(dto.Option2) || string.IsNullOrWhiteSpace(dto.CorrectAnswer))
                {
                    errors.Add($"{itemLabel}: skipped — MainText, Option1, Option2 and CorrectAnswer are required.");
                    skipped++;
                    continue;
                }

                bool mainTextExists = await _context.Questions.AnyAsync(q => q.MainText == dto.MainText);
                bool questionTextExists = !string.IsNullOrWhiteSpace(dto.QuestionText)
                    && await _context.Questions.AnyAsync(q => q.QuestionText == dto.QuestionText);

                if (mainTextExists || questionTextExists)
                {
                    skipped++;
                    continue;
                }

                _context.Questions.Add(new Question
                {
                    Category        = string.IsNullOrEmpty(dto.Category)         ? null : dto.Category,
                    QuestionText    = string.IsNullOrEmpty(dto.QuestionText)     ? null : dto.QuestionText,
                    MainText        = dto.MainText,
                    Image           = string.IsNullOrEmpty(dto.Image)            ? null : dto.Image,
                    Option1         = dto.Option1,
                    Option2         = dto.Option2,
                    Option3         = string.IsNullOrEmpty(dto.Option3)          ? null : dto.Option3,
                    Option4         = string.IsNullOrEmpty(dto.Option4)          ? null : dto.Option4,
                    Option5         = string.IsNullOrEmpty(dto.Option5)          ? null : dto.Option5,
                    CorrectAnswer   = dto.CorrectAnswer,
                    BestAnswersRaw  = string.IsNullOrEmpty(dto.BestAnswersRaw)   ? null : dto.BestAnswersRaw,
                    ExplanationText = string.IsNullOrEmpty(dto.ExplanationText)  ? null : dto.ExplanationText,
                    ExplanationImage= string.IsNullOrEmpty(dto.ExplanationImage) ? null : dto.ExplanationImage
                });
                added++;
            }

            if (added > 0)
                await _context.SaveChangesAsync();

            var summary = $"{added} question(s) imported, {skipped} skipped.";
            if (errors.Count > 0)
                summary += " Errors: " + string.Join(" | ", errors);

            TempData[added > 0 || skipped > 0 ? "SuccessMessage" : "ErrorMessage"] = summary;
            return RedirectToAction(nameof(BulkQuestions));
        }

        [Route("BulkQuestions/UploadCsv")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkQuestionsUploadCsv(Microsoft.AspNetCore.Http.IFormFile file)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a CSV file to upload.";
                return RedirectToAction(nameof(BulkQuestions));
            }

            List<QuestionDto> dtos;
            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                dtos = ParseCsvQuestions(reader);
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Could not parse CSV file: {ex.Message}";
                return RedirectToAction(nameof(BulkQuestions));
            }

            if (dtos.Count == 0)
            {
                TempData["ErrorMessage"] = "The uploaded CSV file is empty or contains no questions.";
                return RedirectToAction(nameof(BulkQuestions));
            }

            var added = 0;
            var skipped = 0;
            var errors = new List<string>();

            for (int i = 0; i < dtos.Count; i++)
            {
                var dto = dtos[i];
                var itemLabel = $"Row {i + 2}";

                if (string.IsNullOrWhiteSpace(dto.MainText) || string.IsNullOrWhiteSpace(dto.Option1)
                    || string.IsNullOrWhiteSpace(dto.Option2) || string.IsNullOrWhiteSpace(dto.CorrectAnswer))
                {
                    errors.Add($"{itemLabel}: skipped — MainText, Option1, Option2 and CorrectAnswer are required.");
                    skipped++;
                    continue;
                }

                bool mainTextExists = await _context.Questions.AnyAsync(q => q.MainText == dto.MainText);
                bool questionTextExists = !string.IsNullOrWhiteSpace(dto.QuestionText)
                    && await _context.Questions.AnyAsync(q => q.QuestionText == dto.QuestionText);

                if (mainTextExists || questionTextExists)
                {
                    skipped++;
                    continue;
                }

                _context.Questions.Add(new Question
                {
                    Category        = string.IsNullOrEmpty(dto.Category)         ? null : dto.Category,
                    QuestionText    = string.IsNullOrEmpty(dto.QuestionText)     ? null : dto.QuestionText,
                    MainText        = dto.MainText,
                    Image           = string.IsNullOrEmpty(dto.Image)            ? null : dto.Image,
                    Option1         = dto.Option1,
                    Option2         = dto.Option2,
                    Option3         = string.IsNullOrEmpty(dto.Option3)          ? null : dto.Option3,
                    Option4         = string.IsNullOrEmpty(dto.Option4)          ? null : dto.Option4,
                    Option5         = string.IsNullOrEmpty(dto.Option5)          ? null : dto.Option5,
                    CorrectAnswer   = dto.CorrectAnswer,
                    BestAnswersRaw  = string.IsNullOrEmpty(dto.BestAnswersRaw)   ? null : dto.BestAnswersRaw,
                    ExplanationText = string.IsNullOrEmpty(dto.ExplanationText)  ? null : dto.ExplanationText,
                    ExplanationImage= string.IsNullOrEmpty(dto.ExplanationImage) ? null : dto.ExplanationImage
                });
                added++;
            }

            if (added > 0)
                await _context.SaveChangesAsync();

            var summary = $"{added} question(s) imported from CSV, {skipped} skipped.";
            if (errors.Count > 0)
                summary += " Errors: " + string.Join(" | ", errors);

            TempData[added > 0 || skipped > 0 ? "SuccessMessage" : "ErrorMessage"] = summary;
            return RedirectToAction(nameof(BulkQuestions));
        }

        private static List<QuestionDto> ParseCsvQuestions(StreamReader reader)
        {
            // RFC-4180 compliant CSV parser that handles quoted fields containing commas/newlines
            static string[] SplitCsvLine(string line)
            {
                var fields = new List<string>();
                var field = new System.Text.StringBuilder();
                bool inQuotes = false;

                for (int i = 0; i < line.Length; i++)
                {
                    char c = line[i];
                    if (inQuotes)
                    {
                        if (c == '"')
                        {
                            if (i + 1 < line.Length && line[i + 1] == '"') { field.Append('"'); i++; }
                            else inQuotes = false;
                        }
                        else field.Append(c);
                    }
                    else
                    {
                        if (c == '"') inQuotes = true;
                        else if (c == ',') { fields.Add(field.ToString()); field.Clear(); }
                        else field.Append(c);
                    }
                }
                fields.Add(field.ToString());
                return fields.ToArray();
            }

            // Expected header order (case-insensitive)
            // Category,QuestionText,MainText,Image,Option1,Option2,Option3,Option4,Option5,CorrectAnswer,BestAnswersRaw,ExplanationText,ExplanationImage
            var headerLine = reader.ReadLine();
            if (headerLine == null)
                return new List<QuestionDto>();

            var headers = SplitCsvLine(headerLine)
                .Select(h => h.Trim().ToLowerInvariant())
                .ToArray();

            int Idx(string name) => System.Array.IndexOf(headers, name.ToLowerInvariant());

            int iCategory        = Idx("Category");
            int iQuestionText    = Idx("QuestionText");
            int iMainText        = Idx("MainText");
            int iImage           = Idx("Image");
            int iOption1         = Idx("Option1");
            int iOption2         = Idx("Option2");
            int iOption3         = Idx("Option3");
            int iOption4         = Idx("Option4");
            int iOption5         = Idx("Option5");
            int iCorrectAnswer   = Idx("CorrectAnswer");
            int iBestAnswersRaw  = Idx("BestAnswersRaw");
            int iExplanationText = Idx("ExplanationText");
            int iExplanationImage= Idx("ExplanationImage");

            string? Get(string[] cols, int idx) =>
                idx >= 0 && idx < cols.Length ? cols[idx].Trim() : null;

            var dtos = new List<QuestionDto>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = SplitCsvLine(line);
                dtos.Add(new QuestionDto(
                    Category:         Get(cols, iCategory),
                    QuestionText:     Get(cols, iQuestionText),
                    MainText:         Get(cols, iMainText) ?? string.Empty,
                    Image:            Get(cols, iImage),
                    Option1:          Get(cols, iOption1) ?? string.Empty,
                    Option2:          Get(cols, iOption2) ?? string.Empty,
                    Option3:          Get(cols, iOption3),
                    Option4:          Get(cols, iOption4),
                    Option5:          Get(cols, iOption5),
                    CorrectAnswer:    Get(cols, iCorrectAnswer) ?? string.Empty,
                    BestAnswersRaw:   Get(cols, iBestAnswersRaw),
                    ExplanationText:  Get(cols, iExplanationText),
                    ExplanationImage: Get(cols, iExplanationImage)
                ));
            }
            return dtos;
        }

        [Route("BulkQuestions/TemplateCsv")]
        public IActionResult BulkQuestionsTemplateCsv()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Category,QuestionText,MainText,Image,Option1,Option2,Option3,Option4,Option5,CorrectAnswer,BestAnswersRaw,ExplanationText,ExplanationImage");
            sb.AppendLine("Trial,,\"A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?\",q1.png,$0.50,$0.25,$0.75,$0.05,,$0.25,,\"The correct breakdown is $0.25 for the eraser and $0.75 for the pencil.\",q1x.png");
            sb.AppendLine("Causal Reasoning,\"Which answer best explains why the bike-sharing program may not be the real cause of fewer accidents?\",\"A small town introduces a bike-sharing program and traffic accidents decline. However, new traffic lights and road improvements were also installed at the same time.\",,\"Traffic lights and road improvements could explain the decline\",\"Cyclists may still get into accidents\",\"People might dislike cycling in bad weather\",\"Bicycle ownership is unrelated to traffic accidents\",\"The town's population may have decreased\",\"Traffic lights and road improvements could explain the decline\",1,\"Multiple interventions occurred simultaneously.\",");
            // Cognitive Reflection row — note commas inside field values are wrapped in double-quotes
            sb.AppendLine("Cognitive Reflection,,\"A bat and a ball cost $1.10 in total. The bat costs $1 more than the ball. How much does the ball cost?\",,$0.05,$0.10,$0.50,$1.00,,$0.05,,\"The intuitive answer is $0.10, but that would make the bat $1.10 and the total $1.20. The correct answer is $0.05: ball = $0.05, bat = $1.05, total = $1.10.\",");
            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "questions_template.csv");
        }

        [Route("BulkQuestions/Template")]
        public IActionResult BulkQuestionsTemplate()
        {
            var template = new[]
            {
                new QuestionDto(
                    Category: "Trial",
                    QuestionText: null,
                    MainText: "A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?",
                    Image: "q1.png",
                    Option1: "$0.50",
                    Option2: "$0.25",
                    Option3: "$0.75",
                    Option4: "$0.05",
                    Option5: null,
                    CorrectAnswer: "$0.25",
                    BestAnswersRaw: null,
                    ExplanationText: "The correct breakdown is $0.25 for the eraser and $0.75 for the pencil.",
                    ExplanationImage: "q1x.png"
                ),
                new QuestionDto(
                    Category: "Causal Reasoning",
                    QuestionText: "Which answer best explains why the bike-sharing program may not be the real cause of fewer accidents?",
                    MainText: "A small town introduces a bike-sharing program and traffic accidents decline. However, new traffic lights and road improvements were also installed at the same time.",
                    Image: null,
                    Option1: "Traffic lights and road improvements could explain the decline",
                    Option2: "Cyclists may still get into accidents",
                    Option3: "People might dislike cycling in bad weather",
                    Option4: "Bicycle ownership is unrelated to traffic accidents",
                    Option5: "The town's population may have decreased",
                    CorrectAnswer: "Traffic lights and road improvements could explain the decline",
                    BestAnswersRaw: "1",
                    ExplanationText: "Multiple interventions occurred simultaneously.",
                    ExplanationImage: null
                )
            };

            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(template, JsonOptions);
            return File(bytes, "application/json", "questions_template.json");
        }

        [Route("BulkQuestions/RemoveAll")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAllQuestions()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var questions = await _context.Questions.OrderBy(q => q.Category).ThenBy(q => q.ID).ToListAsync();
            var bytes = QuestionsToJsonBytes(questions);

            _context.Questions.RemoveRange(questions);
            await _context.SaveChangesAsync();

            return File(bytes, "application/json", $"questions_backup_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        }

        // ========== FEEDBACK ==========

        [Route("Feedback")]
        public async Task<IActionResult> Feedback()
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var feedbacks = await _context.Feedbacks
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            ViewBag.SystemAdmins = await _context.Users
                .Where(u => u.AuthLevel == AuthLevel.SystemAdmin)
                .OrderBy(u => u.Name)
                .Select(u => u.Name)
                .ToListAsync();
            ViewBag.CurrentAdminName = appUser.Name;

            return View("~/Views/Admin/SystemAdmin/Feedback.cshtml", feedbacks);
        }

        [Route("Feedback/{id}")]
        [HttpGet]
        public async Task<IActionResult> FeedbackDetail(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            var systemAdmins = await _context.Users
                .Where(u => u.AuthLevel == AuthLevel.SystemAdmin)
                .OrderBy(u => u.Name)
                .Select(u => u.Name)
                .ToListAsync();

            var currentAdmin = await GetCurrentUserAsync();

            return Json(new
            {
                feedback.ID,
                feedback.Name,
                feedback.Email,
                Type = feedback.Type.ToString(),
                feedback.Message,
                feedback.ImageFileName,
                CreatedAt = feedback.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                feedback.AssignedTo,
                feedback.AdminReply,
                RepliedAt = feedback.RepliedAt?.ToString("yyyy-MM-dd HH:mm"),
                feedback.TenantCode,
                SystemAdmins = systemAdmins,
                CurrentAdminName = currentAdmin?.Name
            });
        }

        [Route("Feedback/{id}/Assign")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignFeedback(int id, string assignedTo)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            feedback.AssignedTo = assignedTo?.Trim();
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Feedback #{id} assigned to {feedback.AssignedTo}.";
            return RedirectToAction(nameof(Feedback));
        }

        [Route("Feedback/{id}/Remove")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFeedback(int id)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            // Delete attached image if present
            if (!string.IsNullOrEmpty(feedback.ImageFileName))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "feedback", feedback.ImageFileName);
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);
            }

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Feedback #{id} has been removed.";
            return RedirectToAction(nameof(Feedback));
        }

        [Route("Feedback/{id}/Reply")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyFeedback(int id, string replyMessage)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(replyMessage))
            {
                TempData["ErrorMessage"] = "Reply message cannot be empty.";
                return RedirectToAction(nameof(Feedback));
            }

            feedback.AdminReply = replyMessage.Trim();
            feedback.RepliedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Send reply email
            try
            {
                var tenant = HttpContext.Items["Tenant"] as TenantConfig;
                var siteName = tenant?.SiteName ?? "AI Relief";
                var subject = $"Response to your {feedback.Type.ToString().ToLowerInvariant()} — {siteName}";
                var body = $"Dear {feedback.Name},\n\n"
                    + $"Thank you for your recent {feedback.Type.ToString().ToLowerInvariant()}. A member of our team has reviewed it and would like to share the following response:\n\n"
                    + $"{feedback.AdminReply}\n\n"
                    + "If you have any further questions or concerns, please do not hesitate to get in touch.\n\n"
                    + "Kind regards,\n"
                    + $"The {siteName} Team";

                await _emailService.SendAsync(feedback.Email, subject, body);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to send reply email: {ex.Message}");
            }

            TempData["SuccessMessage"] = $"Reply sent to {feedback.Email}.";
            return RedirectToAction(nameof(Feedback));
        }

        // ========== CACHE INVALIDATION ==========

        [Route("EvictCache")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvictCache(CancellationToken cancellationToken)
        {
            var appUser = await GetCurrentUserAsync();
            if (appUser?.AuthLevel != AuthLevel.SystemAdmin)
                return Forbid();

            await _outputCacheStore.EvictByTagAsync("market-pages", cancellationToken);
            await _outputCacheStore.EvictByTagAsync("static-pages", cancellationToken);

            TempData["SuccessMessage"] = "Output cache has been cleared.";
            return RedirectToAction(nameof(Groups));
        }
    }
}
