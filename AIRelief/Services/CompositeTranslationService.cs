using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AIRelief.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace AIRelief.Services
{
    public class CompositeTranslationService
    {
        private readonly AIReliefContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizer<AIRelief.LanguageResources.SharedLayoutResource> _json;
        private readonly IMemoryCache _cache;

        public CompositeTranslationService(
            AIReliefContext db,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<AIRelief.LanguageResources.SharedLayoutResource> json,
            IMemoryCache cache)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _json = json;
            _cache = cache;
        }

        private TenantConfig Tenant
            => _httpContextAccessor.HttpContext?.Items["Tenant"] as TenantConfig
               ?? throw new InvalidOperationException("No tenant resolved for this request");

        public string Get(string key)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var market = Tenant.MarketCode;

            var translations = GetMarketTranslations(market, lang);

            if (translations.TryGetValue(key, out var value))
                return value;

            var jsonValue = _json[key];
            return jsonValue.ResourceNotFound ? key : jsonValue.Value;
        }

        private Dictionary<string, string> GetMarketTranslations(string market, string lang)
        {
            var cacheKey = $"translations:{market}:{lang}";

            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                // ONE query loads ALL translations for this market+language
                var dbTranslations = _db.Translations
                    .Where(t => t.Language == lang &&
                               (t.Market == market || t.Market == null))
                    .OrderByDescending(t => t.Market) // market-specific wins over null
                    .ToList();

                var result = new Dictionary<string, string>();
                foreach (var t in dbTranslations)
                {
                    // First write wins (market-specific came first due to ordering)
                    result.TryAdd(t.Key, t.Value);
                }

                return result;
            })!;
        }
    }
}
