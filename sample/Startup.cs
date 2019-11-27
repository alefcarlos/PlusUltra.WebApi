using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using PlusUltra.Swagger.Extensions;
using PlusUltra.WebApi.Hosting;

namespace sample
{
    public class Startup : WebApiStartup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
            : base(configuration, env, useAuthentication: true)
        {
        }

        public override void AfterConfigureApp(IApplicationBuilder app)
        {
            app.UseDocumentation(configuration: c =>
            {
                c.DocumentTitle = "Sample WebApi";
            });
        }

        public override void AfterConfigureServices(IServiceCollection services)
        {
            services.AddDocumentation(new OpenApiInfo
            {
                Title = "Sample WebApi"
            });
        }

        public override void BeforeConfigureApp(IApplicationBuilder app)
        {
            if (environment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
                //app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UsePathBase("/v1/sample/");
        }

        public override void ConfigureAfterRouting(IApplicationBuilder app)
        {

        }

        public override void MapEndpoints(IEndpointRouteBuilder endpoints)
        {

        }
    }
}
