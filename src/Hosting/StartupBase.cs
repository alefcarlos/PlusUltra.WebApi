using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlusUltra.Swagger.Extensions;
using PlusUltra.WebApi.HealthCheck;
using PlusUltra.WebApi.JWT;
using PlusUltra.WebApi.Middlewares;
using PlusUltra.WebApi.Versioning;

namespace PlusUltra.WebApi.Hosting
{
    public abstract class StartupBase
    {
        public StartupBase(IConfiguration configuration, ILoggerFactory loggerFactory, bool useAuthentication)
        {
            Configuration = configuration;
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger<StartupBase>();
            this.useAuthentication = useAuthentication;
        }

        protected readonly IConfiguration Configuration;

        protected readonly bool useAuthentication;

        private readonly ILoggerFactory loggerFactory;
        protected readonly ILogger<StartupBase> logger;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (useAuthentication)
                services.AddSecurity(Configuration);

            services.AddCustomCors();

            services.AddHealthCheck();

            services.AddMvc(o =>
            {
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddFluentValidation()
                .AddJsonOptions(o => o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);

            services.AddApiVersion();

            services.AddMetrics();

            AfterConfigureServices(services, loggerFactory);
        }

        public abstract void AfterConfigureServices(IServiceCollection services, ILoggerFactory loggerFactory);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApiVersionDescriptionProvider provider)
        {
            BeforeConfigureApp(app, env);

            if (useAuthentication)
                app.UseAuthentication();

            app.UseCustomCors();

            app.UseHealthCheck();

            app.UseMiddleware<HttpExceptionMiddleware>()
                .UseMvc();

            app.UseDocumentation(provider);

            AfterConfigureApp(app, env);
        }

        public abstract void BeforeConfigureApp(IApplicationBuilder app, IHostingEnvironment env);

        public abstract void AfterConfigureApp(IApplicationBuilder app, IHostingEnvironment env);
    }
}
