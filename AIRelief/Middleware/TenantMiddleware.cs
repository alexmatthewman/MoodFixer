using System.Globalization;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AIRelief.Models;
using Microsoft.AspNetCore.Http;

namespace AIRelief.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TenantConfig _tenant;

        public TenantMiddleware(RequestDelegate next, TenantConfig tenant)
        {
            _next = next;
            _tenant = tenant;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var lang = context.Request.Query["lang"].FirstOrDefault()
                       ?? _tenant.DefaultLanguage;

            var culture = new CultureInfo(lang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            context.Items["Tenant"] = _tenant;

            await _next(context);
        }
    }
}
