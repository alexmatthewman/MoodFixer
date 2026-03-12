using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AIRelief.Caching;
using AIRelief.Localization;
using AIRelief.Data;
using AIRelief.Middleware;
using AIRelief.Models;
using AIRelief.Theming;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
            var tenantsSection = Configuration.GetSection("Tenants");
            var tenantDict = new Dictionary<string, TenantConfig>();

            foreach (var child in tenantsSection.GetChildren())
            {
                var tenant = child.Get<TenantConfig>();
                if (tenant is not null)
                    tenantDict[child.Key] = tenant;
            }

            var registry = new TenantRegistry(tenantDict);
            services.AddSingleton(registry);

            services.AddDbContext<AIReliefContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("IdentityConnection")));

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
            services.AddScoped<AIRelief.Services.EmailTemplateService>();
            services.AddSingleton<AIRelief.Services.IEmailService, AIRelief.Services.SmtpEmailService>();

            services.AddDistributedMemoryCache();

            services.AddDataProtection()
                .SetApplicationName("AIRelief")
                .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"));

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(b => b.NoCache());
                options.AddPolicy("AnonymousOnly", b =>
                {
                    b.AddPolicy<AnonymousOnlyCachePolicy>();
                    b.Expire(TimeSpan.FromMinutes(5));
                    b.Tag("market-pages");
                });
                options.AddPolicy("PublicAlways", b =>
                {
                    b.AddPolicy<PublicCachePolicy>();
                    b.Expire(TimeSpan.FromMinutes(30));
                    b.Tag("static-pages");
                });
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
                app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
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
                    SeedEmailTemplates.Run(context).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred applying database migrations.");
                    throw;
                }
            }

            app.UseResponseCompression();
            app.UseSession();
            app.UseMiddleware<TenantMiddleware>();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var path = ctx.File.Name;
                    if (path.EndsWith(".css") || path.EndsWith(".js"))
                    {
                        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=86400";
                    }
                    else if (path.EndsWith(".png") || path.EndsWith(".jpg") ||
                             path.EndsWith(".svg") || path.EndsWith(".ico") ||
                             path.EndsWith(".webp"))
                    {
                        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=2592000";
                    }
                }
            });

            app.UseAuthentication();
            app.UseOutputCache();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health")
                    .CacheOutput(b => b.NoCache());
                endpoints.MapRazorPages();

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
