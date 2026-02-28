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

        private static readonly string[] CsvHeaders = new[]
        {
            "Category", "QuestionText", "MainText", "Image",
            "Option1", "Option2", "Option3", "Option4", "Option5",
            "CorrectAnswer", "BestAnswersRaw",
            "ExplanationText", "ExplanationImage"
        };

        private static string ToCsvRow(params string?[] fields)
        {
            return string.Join(",", fields.Select(f =>
            {
                if (f == null) return string.Empty;
                if (f.Contains(',') || f.Contains('"') || f.Contains('\n'))
                    return $"\"{f.Replace("\"", "\"\"")}\"";
                return f;
            }));
        }

        private static string QuestionToCsvRow(Question q) =>
            ToCsvRow(q.Category, q.QuestionText, q.MainText, q.Image,
                     q.Option1, q.Option2, q.Option3, q.Option4, q.Option5,
                     q.CorrectAnswer, q.BestAnswersRaw,
                     q.ExplanationText, q.ExplanationImage);

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

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Join(",", CsvHeaders));
            foreach (var q in questions)
                sb.AppendLine(QuestionToCsvRow(q));

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
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
                TempData["ErrorMessage"] = "Please select a CSV file to upload.";
                return RedirectToAction(nameof(BulkQuestions));
            }

            var added = 0;
            var skipped = 0;
            var errors = new System.Collections.Generic.List<string>();

            using var reader = new System.IO.StreamReader(file.OpenReadStream(), System.Text.Encoding.UTF8);
            var headerLine = await reader.ReadLineAsync();
            if (headerLine == null)
            {
                TempData["ErrorMessage"] = "The uploaded file is empty.";
                return RedirectToAction(nameof(BulkQuestions));
            }

            var headers = ParseCsvRow(headerLine);
            int Idx(string name)
            {
                var i = System.Array.FindIndex(headers, h => h.Trim().Equals(name, System.StringComparison.OrdinalIgnoreCase));
                return i;
            }

            int rowNumber = 1;
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                rowNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = ParseCsvRow(line);

                string Get(string name)
                {
                    var i = Idx(name);
                    return (i >= 0 && i < cols.Length) ? cols[i].Trim() : string.Empty;
                }

                var mainText = Get("MainText");
                var option1  = Get("Option1");
                var option2  = Get("Option2");
                var correct  = Get("CorrectAnswer");

                if (string.IsNullOrWhiteSpace(mainText) || string.IsNullOrWhiteSpace(option1)
                    || string.IsNullOrWhiteSpace(option2) || string.IsNullOrWhiteSpace(correct))
                {
                    errors.Add($"Row {rowNumber}: skipped — MainText, Option1, Option2 and CorrectAnswer are required.");
                    skipped++;
                    continue;
                }

                var questionText = Get("QuestionText");

                bool mainTextExists = await _context.Questions.AnyAsync(q => q.MainText == mainText);
                bool questionTextExists = !string.IsNullOrWhiteSpace(questionText)
                    && await _context.Questions.AnyAsync(q => q.QuestionText == questionText);

                if (mainTextExists || questionTextExists)
                {
                    skipped++;
                    continue;
                }

                var question = new Question
                {
                    Category        = Get("Category").NullIfEmpty(),
                    QuestionText    = questionText.NullIfEmpty(),
                    MainText        = mainText,
                    Image           = Get("Image").NullIfEmpty(),
                    Option1         = option1,
                    Option2         = option2,
                    Option3         = Get("Option3").NullIfEmpty(),
                    Option4         = Get("Option4").NullIfEmpty(),
                    Option5         = Get("Option5").NullIfEmpty(),
                    CorrectAnswer   = correct,
                    BestAnswersRaw  = Get("BestAnswersRaw").NullIfEmpty(),
                    ExplanationText = Get("ExplanationText").NullIfEmpty(),
                    ExplanationImage= Get("ExplanationImage").NullIfEmpty()
                };

                _context.Questions.Add(question);
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

        [Route("BulkQuestions/Template")]
        public IActionResult BulkQuestionsTemplate()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Join(",", CsvHeaders));

            sb.AppendLine(ToCsvRow(
                "Trial",
                "",
                "A pencil costs 50 cents more than an eraser. The total cost of both is $1. How much does the eraser cost?",
                "q1.png",
                "$0.50", "$0.25", "$0.75", "$0.05", "",
                "$0.25", "",
                "The correct breakdown is $0.25 for the eraser and $0.75 for the pencil.", "q1x.png"
            ));

            sb.AppendLine(ToCsvRow(
                "Causal Reasoning",
                "Which answer best explains why the bike-sharing program may not be the real cause of fewer accidents?",
                "A small town introduces a bike-sharing program and traffic accidents decline. However, new traffic lights and road improvements were also installed at the same time.",
                "",
                "Traffic lights and road improvements could explain the decline",
                "Cyclists may still get into accidents",
                "People might dislike cycling in bad weather",
                "Bicycle ownership is unrelated to traffic accidents",
                "The town's population may have decreased",
                "Traffic lights and road improvements could explain the decline",
                "1",
                "Multiple interventions occurred simultaneously.", ""
            ));

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "questions_template.csv");
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

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Join(",", CsvHeaders));
            foreach (var q in questions)
                sb.AppendLine(QuestionToCsvRow(q));

            _context.Questions.RemoveRange(questions);
            await _context.SaveChangesAsync();

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"questions_backup_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        private static string[] ParseCsvRow(string line)
        {
            var fields = new System.Collections.Generic.List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            fields.Add(current.ToString());
            return fields.ToArray();
        }
    }
}
