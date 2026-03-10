using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace AIRelief.Localization
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();
        private readonly string _localesPath;
        private readonly string _namespace;

        public JsonStringLocalizer(string localesPath, string ns = null)
        {
            _localesPath = localesPath;
            _namespace = ns;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = GetString(name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value is null);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var value = GetString(name);
                var formatted = value is not null ? string.Format(value, arguments) : name;
                return new LocalizedString(name, formatted, resourceNotFound: value is null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var strings = LoadAllForLanguage(lang);
            return strings.Select(kvp => new LocalizedString(kvp.Key, kvp.Value));
        }

        private string GetString(string key)
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var strings = _namespace is not null
                ? LoadNamespace(lang, _namespace)
                : LoadAllForLanguage(lang);

            if (strings.TryGetValue(key, out var value))
                return value;

            // Fallback to English
            if (lang != "en")
            {
                var fallback = _namespace is not null
                    ? LoadNamespace("en", _namespace)
                    : LoadAllForLanguage("en");

                if (fallback.TryGetValue(key, out var fbValue))
                    return fbValue;
            }

            return null;
        }

        private Dictionary<string, string> LoadNamespace(string lang, string ns)
        {
            var cacheKey = $"{lang}:{ns}";
            return _cache.GetOrAdd(cacheKey, _ =>
            {
                var filePath = Path.Combine(_localesPath, lang, $"{ns}.json");
                return LoadJsonFile(filePath);
            });
        }

        private Dictionary<string, string> LoadAllForLanguage(string lang)
        {
            var cacheKey = $"{lang}:*";
            return _cache.GetOrAdd(cacheKey, _ =>
            {
                var langDir = Path.Combine(_localesPath, lang);
                var merged = new Dictionary<string, string>();

                if (!Directory.Exists(langDir))
                    return merged;

                foreach (var file in Directory.GetFiles(langDir, "*.json", SearchOption.AllDirectories))
                {
                    foreach (var kvp in LoadJsonFile(file))
                        merged[kvp.Key] = kvp.Value;
                }

                return merged;
            });
        }

        private static Dictionary<string, string> LoadJsonFile(string path)
        {
            if (!File.Exists(path))
                return new Dictionary<string, string>();

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
    }
}
