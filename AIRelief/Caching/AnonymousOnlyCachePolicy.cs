using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OutputCaching;

namespace AIRelief.Caching
{
    public class AnonymousOnlyCachePolicy : IOutputCachePolicy
    {
        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var isAuthenticated = context.HttpContext.User?.Identity?.IsAuthenticated == true;

            context.EnableOutputCaching = !isAuthenticated;
            context.AllowCacheLookup = !isAuthenticated;
            context.AllowCacheStorage = !isAuthenticated;
            context.AllowLocking = true;

            context.CacheVaryByRules.VaryByHost = true;

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
