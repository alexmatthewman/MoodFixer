using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Primitives;
using AIRelief.Models;
using WebEssentials.AspNetCore.Pwa;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.IO;
using System;
using Microsoft.AspNetCore.Identity;

namespace AIRelief
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add session services
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "AIRelief.Session";
            });

            services.AddMvc();
            services.AddDbContext<AIReliefContext>(options => options.UseSqlite(Configuration.GetConnectionString("IdentityConnection")));

            services.AddDefaultIdentity<IdentityUser>(options =>
            {
                // TODO This disables email sending for dev - needed for PROD
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddRoles<IdentityRole>() // if you need roles
            .AddEntityFrameworkStores<AIReliefContext>();


            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            services.AddAuthentication()
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                })
                .AddMicrosoftAccount(microsoftOptions =>
                {
                    microsoftOptions.ClientId = Configuration["Authentication:Microsoft:ClientId"];
                    microsoftOptions.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                });

            services.AddRazorPages();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddProgressiveWebApp();
            services.AddControllersWithViews();
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;

            });

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = System.TimeSpan.FromDays(60);
                options.ExcludedHosts.Add("fixmymood.com");
                options.ExcludedHosts.Add("www.fixmymood.com");

            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseHsts(); //https://asp.net-hacker.rocks/2018/07/09/aspnetcore-ssl.html
            app.UseStaticFiles();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseCors();

            // Add session middleware - must be after UseRouting and before UseAuthentication
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardingOptions.KnownNetworks.Clear(); //its loopback by default
            forwardingOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardingOptions);
            app.UseResponseCompression();
            app.UseCookiePolicy();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default", "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages(); // Must be here to enable Identity pagesd
            });
        }
    }
}