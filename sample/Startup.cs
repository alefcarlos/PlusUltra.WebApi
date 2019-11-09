using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using PlusUltra.Swagger.Extensions;
using PlusUltra.WebApi.Hosting;

namespace sample
{
    public class Startup : WebApiStartup
    {
        public Startup(IConfiguration configuration)
            : base(configuration, useAuthentication: true, jwtConfigureOptions: null)
        {
        }

        public override void AfterConfigureApp(IApplicationBuilder app, IWebHostEnvironment env)
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

        public override void BeforeConfigureApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
                //app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
        }

        public override void ConfigureAfterRouting(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }

        public override void MapEndpoints(IEndpointRouteBuilder endpoints)
        {

        }
    }
}
