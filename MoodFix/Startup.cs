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
using Microsoft.Extensions.Primitives;
using MoodFix.Models;
using WebEssentials.AspNetCore.Pwa;
using Microsoft.Data.Sqlite;
using SQLitePCL;
using System.IO;

namespace MoodFix
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


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //services.AddDbContext<MoodFixContext>(options =>
            //        options.UseMySql(Configuration.GetConnectionString("IdentityConnection")));
          
            services.AddDbContext<MoodFixContext>(options => options.UseSqlite(Configuration.GetConnectionString("IdentityConnection")));
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddProgressiveWebApp();

            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            
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

            app.UseHttpsRedirection();

            var forwardingOptions = new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardingOptions.KnownNetworks.Clear(); //its loopback by default
            forwardingOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardingOptions);


            app.UseResponseCompression();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
