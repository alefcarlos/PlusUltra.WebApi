using System;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlusUltra.WebApi.JWT;
using Microsoft.AspNetCore.Hosting;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using PlusUltra.WebApi.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using App.Metrics;
using App.Metrics.Formatters.Prometheus;
using System.Linq;

namespace PlusUltra.WebApi.Hosting
{
    public abstract class WebApiStartup
    {
        public WebApiStartup(IConfiguration configuration, IWebHostEnvironment environment, bool useAuthentication, Action<JwtBearerOptions> jwtConfigureOptions = null)
        {
            Configuration = configuration;
            this.useAuthentication = useAuthentication;
            this.jwtConfigureOptions = jwtConfigureOptions;
            this.environment = environment;
        }

        protected readonly IConfiguration Configuration;
        protected readonly IWebHostEnvironment environment;

        protected readonly bool useAuthentication;

        private readonly Action<JwtBearerOptions> jwtConfigureOptions;


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (useAuthentication)
                services.AddSecurity(Configuration, environment.IsProduction(), jwtConfigureOptions);

            services.AddHealthChecks();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                })
                .AddFluentValidation();

            // https://benfoster.io/blog/injecting-urlhelper-in-aspnet-core-mvc
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x =>
            {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });


            var metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusPlainText()
                .OutputMetrics.AsPrometheusProtobuf()
                .Build();

            DiagnosticSourceAdapter.StartListening(metrics, new DiagnosticSourceAdapterOptions
            {
                ListenerFilterPredicate = (d) => true
            });

            services.AddMetricsTrackingMiddleware();
            services.AddMetricsReportingHostedService();
            services.AddMetricsEndpoints(options =>
            {
                options.MetricsEndpointOutputFormatter = metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
            });

            services.AddMetrics(metrics);

            AfterConfigureServices(services);
        }

        public abstract void AfterConfigureServices(IServiceCollection services);

        public abstract void BeforeConfigureApp(IApplicationBuilder app);

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILogger<WebApiStartup> logger)
        {
            app.UseMetricsAllMiddleware();
            app.UseErrorMiddleware(environment);

            BeforeConfigureApp(app);

            app.UseRouting();

            app.UseAppMetricsEndpointRoutesResolver();

            ConfigureAfterRouting(app);

            if (useAuthentication)
            {
                app.UseAuthorization();
                app.UseAuthentication();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapGet("/liveness", async context =>
                {
                    await context.Response.WriteAsync("Ok");
                });

                MapEndpoints(endpoints);
            });

            AfterConfigureApp(app);

            app.UseMetricsAllEndpoints();
        }

        public abstract void ConfigureAfterRouting(IApplicationBuilder app);

        public abstract void MapEndpoints(IEndpointRouteBuilder endpoints);

        public abstract void AfterConfigureApp(IApplicationBuilder app);
    }
}
