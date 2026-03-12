using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AIRelief.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace AIRelief.Services
{
    public class EmailTemplateService
    {
        private readonly AIReliefContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _cache;

        public EmailTemplateService(
            AIReliefContext db,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache cache)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        private TenantConfig Tenant
            => _httpContextAccessor.HttpContext?.Items["Tenant"] as TenantConfig
               ?? throw new InvalidOperationException("No tenant resolved for this request");

        /// <summary>
        /// Loads a template by key for the current tenant and culture, falling back to English if needed.
        /// Returns null when no template exists for the given key.
        /// </summary>
        public EmailTemplate? Get(string templateKey)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var market = Tenant.MarketCode;

            var template = LoadTemplate(templateKey, market, lang);

            // Fallback to English
            if (template is null && lang != "en")
                template = LoadTemplate(templateKey, market, "en");

            return template;
        }

        /// <summary>
        /// Renders a template by replacing placeholders with provided values.
        /// </summary>
        public (string Subject, string Body)? Render(string templateKey, Dictionary<string, string> placeholders)
        {
            var template = Get(templateKey);
            if (template is null)
                return null;

            var subject = ReplacePlaceholders(template.Subject, placeholders);
            var body = ReplacePlaceholders(template.Body, placeholders);

            return (subject, body);
        }

        private EmailTemplate? LoadTemplate(string templateKey, string market, string lang)
        {
            var cacheKey = $"emailtemplate:{templateKey}:{market}:{lang}";

            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                // Market-specific first, then fallback to null market
                return _db.EmailTemplates
                    .Where(t => t.TemplateKey == templateKey && t.Language == lang &&
                                (t.Market == market || t.Market == null))
                    .OrderByDescending(t => t.Market)
                    .FirstOrDefault();
            });
        }

        private static string ReplacePlaceholders(string text, Dictionary<string, string> placeholders)
        {
            foreach (var (key, value) in placeholders)
            {
                text = text.Replace($"{{{key}}}", value);
            }
            return text;
        }
    }
}
