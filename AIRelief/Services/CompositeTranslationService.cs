using System;
using System.Globalization;
using System.Linq;
using AIRelief.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AIRelief.Services
{
    public class CompositeTranslationService
    {
        private readonly AIReliefContext _db;
        private readonly TenantConfig _tenant;
        private readonly IMemoryCache _cache;

        public CompositeTranslationService(
            AIReliefContext db,
            TenantConfig tenant,
            IMemoryCache cache)
        {
            _db = db;
            _tenant = tenant;
            _cache = cache;
        }

        public string Get(string key)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var market = _tenant.MarketCode;
            var cacheKey = $"tr:{market}:{lang}:{key}";

            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                // 1. Market-specific match (exact market + language)
                // 2. Shared/global match (market is null, same language)
                var dbValue = _db.Translations
                    .Where(t => t.Key == key && t.Language == lang)
                    .OrderByDescending(t => t.Market == market)
                    .ThenByDescending(t => t.Market == null)
                    .Select(t => t.Value)
                    .FirstOrDefault();

                if (dbValue is not null)
                    return dbValue;

                // 3. English fallback from DB (if current language is not English)
                if (lang != "en")
                {
                    var enValue = _db.Translations
                        .Where(t => t.Key == key && t.Language == "en")
                        .OrderByDescending(t => t.Market == market)
                        .ThenByDescending(t => t.Market == null)
                        .Select(t => t.Value)
                        .FirstOrDefault();

                    if (enValue is not null)
                        return enValue;
                }

                // 4. Return the key itself as a last resort
                return key;
            })!;
        }
    }
}
