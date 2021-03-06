using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web.UI;
using BlazorClient.Services.Job;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.ApplicationInsights.Extensibility;
using BlazorClient.Extension;
using BlazorClient.Services.Valet;

namespace BlazorClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddApplicationInsightsTelemetry();

            services.AddMicrosoftIdentityWebAppAuthentication(Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { Configuration["Api:JobApiScope"]})                    
                    //.AddInMemoryTokenCaches();
                    .AddDistributedTokenCaches();

            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = Configuration["Redis:Name"];
                options.Configuration = Configuration["Redis:ConnectionString"];
            });

            services.AddHttpClient<IJobService,JobService>();
            services.AddHttpClient<IValetService, ValetService>();

            services.AddHttpContextAccessor();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitalizer>();
#if !DEBUG
            services.AddSignalR().AddAzureSignalR(o =>
            {
                o.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required;
                o.ConnectionString = Configuration["SignalRService"];
            });
#endif
            services.AddControllersWithViews().AddMicrosoftIdentityUI();

            services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy
                options.FallbackPolicy = options.DefaultPolicy;
            });

            services.AddRazorPages();

            services.AddServerSideBlazor()
                    .AddMicrosoftIdentityConsentHandler();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
