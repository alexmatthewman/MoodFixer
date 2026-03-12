using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRelief.Models;
using AIRelief.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIRelief.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var requestPath = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}";
            var tenant = context.Items["Tenant"] as TenantConfig;
            var tenantCode = tenant?.MarketCode ?? "relief";
            var siteName = tenant?.SiteName ?? "AI Relief";
            var userName = context.User?.Identity?.Name ?? "Anonymous";

            var detailBuilder = new StringBuilder();
            detailBuilder.AppendLine($"Page/URL: {requestPath}");
            detailBuilder.AppendLine($"User: {userName}");
            detailBuilder.AppendLine($"Tenant: {tenantCode}");
            detailBuilder.AppendLine($"Timestamp: {DateTime.UtcNow:O}");
            detailBuilder.AppendLine($"TraceIdentifier: {context.TraceIdentifier}");
            detailBuilder.AppendLine();
            detailBuilder.AppendLine($"Exception Type: {ex.GetType().FullName}");
            detailBuilder.AppendLine($"Message: {ex.Message}");
            detailBuilder.AppendLine();
            detailBuilder.AppendLine("Stack Trace:");
            detailBuilder.AppendLine(ex.StackTrace);

            if (ex.InnerException is not null)
            {
                detailBuilder.AppendLine();
                detailBuilder.AppendLine($"Inner Exception Type: {ex.InnerException.GetType().FullName}");
                detailBuilder.AppendLine($"Inner Exception Message: {ex.InnerException.Message}");
                detailBuilder.AppendLine("Inner Exception Stack Trace:");
                detailBuilder.AppendLine(ex.InnerException.StackTrace);
            }

            var fullDetails = detailBuilder.ToString();

            // Truncate to fit the Feedback.Message 5000-char limit
            var feedbackMessage = fullDetails.Length > 5000
                ? fullDetails[..4997] + "..."
                : fullDetails;

            try
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AIReliefContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                // Save as a Feedback bug entry
                var feedback = new Feedback
                {
                    Name = "System",
                    Email = "noreply@system.local",
                    Type = FeedbackType.Bug,
                    Message = feedbackMessage,
                    TenantCode = tenantCode
                };

                db.Feedbacks.Add(feedback);
                await db.SaveChangesAsync();

                // Email all SystemAdmin users
                var adminEmails = await db.Users
                    .Where(u => u.AuthLevel == AuthLevel.SystemAdmin)
                    .Select(u => u.Email)
                    .ToListAsync();

                if (adminEmails.Count > 0)
                {
                    var subject = $"[{siteName}] Unhandled Error – {ex.GetType().Name}";
                    var body = $"An unhandled error occurred on {siteName}.\n\n{fullDetails}";

                    foreach (var email in adminEmails)
                    {
                        try
                        {
                            await emailService.SendAsync(email, subject, body);
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, "Failed to send error notification to {Email}.", email);
                        }
                    }
                }
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log unhandled exception to database or send admin emails.");
            }

            // Redirect to the generic error page
            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = 500;
                context.Response.Redirect("/Home/Error");
            }
        }
    }
}
