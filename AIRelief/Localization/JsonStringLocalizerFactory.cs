using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace AIRelief.Localization
{
    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly string _localesPath;
        private readonly ILogger<JsonStringLocalizer> _logger;

        public JsonStringLocalizerFactory(IWebHostEnvironment env, ILogger<JsonStringLocalizer> logger)
        {
            _localesPath = Path.Combine(env.ContentRootPath, "LanguageResources", "Locales");
            _logger = logger;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            // Map marker class name to a relative path inside Locales/{lang}/.
            // e.g. SharedLayoutResource   ? Shared/layout
            //      HomeIndexResource       ? Home/index
            //      AdminGroupAdminIndexResource ? Admin/GroupAdmin/index
            var ns = MapTypeToNamespace(resourceSource.Name);
            return new JsonStringLocalizer(_localesPath, ns, _logger);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            var ns = baseName.Split('.').Last();
            ns = MapTypeToNamespace(ns);
            return new JsonStringLocalizer(_localesPath, ns, _logger);
        }

        private static string MapTypeToNamespace(string typeName)
        {
            // Strip the "Resource" suffix
            const string suffix = "Resource";
            if (typeName.EndsWith(suffix, StringComparison.Ordinal))
                typeName = typeName[..^suffix.Length];

            // Known two-segment prefixes that should be kept together
            var twoSegmentPrefixes = new[]
            {
                "AdminGroupAdmin",
                "AdminSystemAdmin",
                "AdminUserAdmin"
            };

            foreach (var prefix in twoSegmentPrefixes)
            {
                if (typeName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    var remainder = typeName[prefix.Length..];
                    var folder = prefix.Insert(5, Path.DirectorySeparatorChar.ToString());
                    var file = char.ToLowerInvariant(remainder[0]) + remainder[1..];
                    return Path.Combine(folder, file);
                }
            }

            // Single-segment prefix: split at the first uppercase boundary after the first char
            for (int i = 1; i < typeName.Length; i++)
            {
                if (char.IsUpper(typeName[i]))
                {
                    var folder = typeName[..i];
                    var file = char.ToLowerInvariant(typeName[i]) + typeName[(i + 1)..];
                    return Path.Combine(folder, file);
                }
            }

            // No split found — use as-is, lowercased
            return typeName.ToLowerInvariant();
        }
    }
}
