using System;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlusUltra.Swagger.Extensions;
using PlusUltra.WebApi.JWT;
using PlusUltra.WebApi.Versioning;
using Microsoft.AspNetCore.Hosting;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PlusUltra.WebApi.Middlewares;

namespace PlusUltra.WebApi.Hosting
{
    public abstract class WebApiStartup
    {
        public WebApiStartup(IConfiguration configuration, bool useAuthentication, Action<JwtBearerOptions> jwtConfigureOptions = null)
        {
            Configuration = configuration;
            this.useAuthentication = useAuthentication;
            this.jwtConfigureOptions = jwtConfigureOptions;
        }

        protected readonly IConfiguration Configuration;

        protected readonly bool useAuthentication;

        private readonly Action<JwtBearerOptions> jwtConfigureOptions;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (useAuthentication)
                services.AddSecurity(Configuration, jwtConfigureOptions);

            services.AddHealthChecks();

            services.AddControllers
                ().AddFluentValidation();

            services.AddApiVersion();

            services.AddMetrics();

            AfterConfigureServices(services);
        }

        public abstract void AfterConfigureServices(IServiceCollection services);

        public abstract void BeforeConfigureApp(IApplicationBuilder app, IWebHostEnvironment env);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider, ILogger<WebApiStartup> logger )
        {
            app.UseErrorMiddleware(env);
            
            BeforeConfigureApp(app, env);

            app.UseRouting();

            ConfigureAfterRouting(app, env);

            if (useAuthentication)
            {
                app.UseAuthorization();
                app.UseAuthentication();
            }

            app.UseDocumentation(provider);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                MapEndpoints(endpoints);
            });

            AfterConfigureApp(app, env);
        }

        public abstract void ConfigureAfterRouting(IApplicationBuilder app, IWebHostEnvironment env);

        public abstract void MapEndpoints(IEndpointRouteBuilder endpoints);

        public abstract void AfterConfigureApp(IApplicationBuilder app, IWebHostEnvironment env);
    }
}
