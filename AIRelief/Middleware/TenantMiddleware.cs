using System.Globalization;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AIRelief.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIRelief.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TenantRegistry _registry;
        private readonly string _fallbackCode;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(
            RequestDelegate next,
            TenantRegistry registry,
            IConfiguration config,
            ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _registry = registry;
            _fallbackCode = config["FallbackTenant"] ?? "relief";
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var host = context.Request.Host.Host;

            var tenant = _registry.Resolve(host)
                         ?? _registry.GetByCode(_fallbackCode);

            var requestedLang = context.Request.Query["lang"].FirstOrDefault();
            var lang = (requestedLang != null && tenant.SupportedLanguages.Contains(requestedLang))
                       ? requestedLang
                       : tenant.DefaultLanguage;

            var culture = new CultureInfo(lang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            context.Items["Tenant"] = tenant;

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["Market"] = tenant.MarketCode,
                ["Language"] = lang
            }))
            {
                await _next(context);
            }
        }
    }
}
