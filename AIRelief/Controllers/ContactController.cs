using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using AIRelief.Models;
using AIRelief.Services;

namespace AIRelief.Controllers
{
    public class ContactController : Controller
    {
        private readonly AIReliefContext _context;
        private readonly IEmailService _emailService;
        private readonly EmailTemplateService _templateService;
        private readonly ILogger<ContactController> _logger;

        // 2 MB limit
        private const long MaxImageSize = 2 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png" };

        public ContactController(AIReliefContext context, IEmailService emailService, EmailTemplateService templateService, ILogger<ContactController> logger)
        {
            _context = context;
            _emailService = emailService;
            _templateService = templateService;
            _logger = logger;
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
            return View("~/Views/Contact/Index.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string name, string email, FeedbackType type, string message, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("Index");
            }

            string? savedFileName = null;

            if (image is not null && image.Length > 0)
            {
                // Validate size
                if (image.Length > MaxImageSize)
                {
                    TempData["Error"] = "Image must be under 2 MB.";
                    return RedirectToAction("Index");
                }

                // Validate extension
                var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext))
                {
                    TempData["Error"] = "Only JPG and PNG images are allowed.";
                    return RedirectToAction("Index");
                }

                // Validate MIME type
                if (!AllowedMimeTypes.Contains(image.ContentType.ToLowerInvariant()))
                {
                    TempData["Error"] = "Only JPG and PNG images are allowed.";
                    return RedirectToAction("Index");
                }

                // Validate image header bytes
                if (!await IsValidImageAsync(image))
                {
                    TempData["Error"] = "The uploaded file is not a valid image.";
                    return RedirectToAction("Index");
                }

                // Save to disk
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "feedback");
                Directory.CreateDirectory(uploadsDir);

                savedFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, savedFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);
            }

            var feedback = new Feedback
            {
                Name = name.Trim(),
                Email = email.Trim(),
                Type = type,
                Message = message.Trim(),
                ImageFileName = savedFileName,
                TenantCode = GetCurrentTenantCode()
            };

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            // Send acknowledgment email
            try
            {
                var tenant = HttpContext.Items["Tenant"] as TenantConfig;
                var siteName = tenant?.SiteName ?? "AI Relief";

                var placeholders = new Dictionary<string, string>
                {
                    ["Name"] = feedback.Name,
                    ["SiteName"] = siteName,
                    ["FeedbackType"] = feedback.Type.ToString().ToLowerInvariant()
                };

                var rendered = _templateService.Render("FeedbackAcknowledgment", placeholders);

                if (rendered is not null)
                {
                    await _emailService.SendAsync(feedback.Email, rendered.Value.Subject, rendered.Value.Body);
                }
                else
                {
                    // Fallback if no template found in DB
                    var subject = $"Thank you for contacting {siteName}";
                    var body = $"Dear {feedback.Name},\n\n"
                        + $"Thank you for reaching out to {siteName}. We have received your {feedback.Type.ToString().ToLowerInvariant()} and a member of our team will review it shortly.\n\n"
                        + "We aim to respond to all correspondence in a timely manner and appreciate your patience.\n\n"
                        + "Kind regards,\n"
                        + $"The {siteName} Team";

                    await _emailService.SendAsync(feedback.Email, subject, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send acknowledgment email for feedback {Id}.", feedback.ID);
            }

            TempData["Success"] = "Thank you for your feedback. We will be in touch soon.";
            return RedirectToAction("Index");
        }

        private static async Task<bool> IsValidImageAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            var header = new byte[8];
            var bytesRead = await stream.ReadAsync(header, 0, header.Length);
            if (bytesRead < 4) return false;

            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                return true;

            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (bytesRead >= 8 &&
                header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                return true;

            return false;
        }
    }
}
