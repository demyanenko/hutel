using System;
using hutel.Filters;
using hutel.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace hutel
{
    public class Startup
    {
        private const string _envUseBasicAuth = "HUTEL_USE_BASIC_AUTH";
        private const string _envUseGoogleAuth = "HUTEL_USE_GOOGLE_AUTH";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.DateParseHandling = DateParseHandling.None;
                    opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    opt.SerializerSettings.Formatting = Formatting.Indented;
                    opt.SerializerSettings.ContractResolver =
                        new CamelCasePropertyNamesContractResolver();
                });
            services.AddScoped<ValidateModelStateAttribute>();
            services.AddLogging(opt => opt.AddConsole());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRedirectToHttpsMiddleware();
            app.UseStaticFiles();
            if (Environment.GetEnvironmentVariable(_envUseBasicAuth) == "1")
            {
                app.UseBasicAuthMiddleware();
            }
            if (Environment.GetEnvironmentVariable(_envUseGoogleAuth) == "1")
            {
                app.UseGoogleAuthMiddleware();
            }
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}
