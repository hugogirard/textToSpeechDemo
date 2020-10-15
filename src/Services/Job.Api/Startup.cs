using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Infrastructure.Shared.Model;
using Infrastructure.Shared.Repository.Document;
using Infrastructure.Shared.Services;

namespace Job.Api
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

            services.AddApplicationInsightsTelemetry();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(Configuration);

            services.AddSingleton<IDocumentRepository<Infrastructure.Shared.Model.Job>,CosmosDbRepository<Infrastructure.Shared.Model.Job>>();
            services.AddSingleton<IQueueService, ServiceBusQueueService>();
            services.AddControllers().AddNewtonsoftJson();

            services.AddSwaggerGen(c => 
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Job API",
                    Description = "Process Job for VoiceMail"                    
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });

            services.AddCors(o => o.AddPolicy("default", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger(c => 
            { 
                c.SerializeAsV2 = true;
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => 
                {
                    swaggerDoc.Servers = new List<OpenApiServer>
                    { new OpenApiServer
                      {
                        Url = $"{httpReq.Scheme}://{httpReq.Host.Value}"
                      } 
                    };
                });
            });
 
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Job API V1");
                c.RoutePrefix = string.Empty;
            });

            //if (env.IsDevelopment())
            //{
                app.UseDeveloperExceptionPage();
            //}

            app.UseCors("default");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
