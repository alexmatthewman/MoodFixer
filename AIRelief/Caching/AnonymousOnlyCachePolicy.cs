using System.Threading;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OutputCaching;

namespace AIRelief.Caching
{
    public class AnonymousOnlyCachePolicy : IOutputCachePolicy
    {
        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated == true;
            var theme = context.HttpContext.Request.Cookies["theme"];

            context.EnableOutputCaching = !isAuthenticated;
            context.AllowCacheLookup = !isAuthenticated;
            context.AllowCacheStorage = !isAuthenticated;
            context.AllowLocking = true;

            context.CacheVaryByRules.VaryByHost = true;
            context.CacheVaryByRules.VaryByValues["theme"] = string.Equals(theme, "light", StringComparison.OrdinalIgnoreCase) ? "light" : "dark";

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated == true;

            context.AllowCacheLookup = !isAuthenticated;

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated == true;

            context.AllowCacheStorage = !isAuthenticated;

            return ValueTask.CompletedTask;
        }
    }
}
