using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AIRelief.Localization;
using AIRelief.Data;
using AIRelief.Middleware;
using AIRelief.Models;
using AIRelief.Theming;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;

namespace AIRelief
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var tenantConfig = Configuration.GetSection("Tenant").Get<TenantConfig>() ?? new TenantConfig();
            services.AddSingleton(tenantConfig);

            services.AddDbContext<AIReliefContext>(options => options.UseNpgsql(Configuration.GetConnectionString("IdentityConnection")));

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AIReliefContext>();

            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            services.AddRazorPages();
            // Register the theme-aware view location expander so themed views override defaults
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new MarketViewLocationExpander());
            });
            // Enable runtime compilation so views placed under /Views are discovered at runtime (useful during development
            // and when views aren't precompiled into the app).
            services.AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddRazorRuntimeCompilation();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache();
            services.AddHealthChecks();
            services.AddScoped<AIRelief.Services.AdminAuthorizationService>();
            services.AddScoped<AIRelief.Services.CompositeTranslationService>();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Apply any pending EF Core migrations, then seed initial data
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Startup>>();
                try
                {
                    var context = services.GetRequiredService<AIReliefContext>();
                    context.Database.Migrate();
                    SeedTranslations.Run(context).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred applying database migrations.");
                    throw;
                }
            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseMiddleware<TenantMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
