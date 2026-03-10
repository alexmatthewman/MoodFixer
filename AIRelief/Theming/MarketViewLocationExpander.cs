using System.Collections.Generic;
using AIRelief.Models;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace AIRelief.Theming
{
    public class MarketViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var tenant = context.ActionContext.HttpContext
                .RequestServices.GetRequiredService<TenantConfig>();
            context.Values["theme"] = tenant.ThemeFolder;
        }

        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            if (!context.Values.TryGetValue("theme", out var theme)
                || string.IsNullOrWhiteSpace(theme))
            {
                foreach (var location in viewLocations)
                    yield return location;
                yield break;
            }

            foreach (var location in viewLocations)
            {
                // Try the themed path first, then fall back to the default
                yield return location.Replace("/Views/", $"/Views/Themes/{theme}/");
                yield return location;
            }
        }
    }
}
