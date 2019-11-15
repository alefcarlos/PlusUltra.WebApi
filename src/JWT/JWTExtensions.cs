using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace PlusUltra.WebApi.JWT
{
    public static class JWTExtensions
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration, Action<JwtBearerOptions> jwtConfigureOptions = null)
        {
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            var tokenConfigurations = services.BuildServiceProvider().GetRequiredService<IOptions<JwtSettings>>().Value;

            services.AddAuthentication(authOptions =>
                        {
                            authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        }).AddJwtBearer(options =>
                        {
                            options.Authority = tokenConfigurations.oidc.Authority;

                            options.RequireHttpsMetadata = false;

                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                // Clock skew compensates for server time drift.
                                // We recommend 5 minutes or less:
                                ClockSkew = TimeSpan.FromMinutes(5),
                                RequireSignedTokens = false,
                                // Ensure the token hasn't expired:
                                RequireExpirationTime = true,
                                ValidateLifetime = true,

                                ValidateAudience = true,
                                ValidAudience = tokenConfigurations.oidc.Audience,

                                ValidIssuer = tokenConfigurations.oidc.Issuer
                            };

                            if (jwtConfigureOptions != null)
                                jwtConfigureOptions(options);
                        });

            // Ativa o uso do token como forma de autorizar o acesso
            // a recursos deste projeto
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireClaim(ClaimTypes.NameIdentifier)
                    .RequireClaim(ClaimTypes.Email)
                    .RequireAuthenticatedUser()
                    .Build());
            });

            return services;
        }
    }
}