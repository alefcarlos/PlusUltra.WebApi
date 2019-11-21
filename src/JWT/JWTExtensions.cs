using System;
using System.IdentityModel.Tokens.Jwt;
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
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration, bool isProduction, Action<JwtBearerOptions> jwtConfigureOptions = null)
        {
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
            var tokenConfigurations = services.BuildServiceProvider().GetRequiredService<IOptions<JwtSettings>>().Value;

            var auth = services.AddAuthentication(authOptions =>
                        {
                            authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        }).AddJwtBearer(options =>
                        {
                            //options.Authority = tokenConfigurations.oidc.Authority;
                            options.RequireHttpsMetadata = isProduction;

                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                // Clock skew compensates for server time drift.
                                // We recommend 5 minutes or less:
                                ClockSkew = TimeSpan.FromMinutes(5),
                                RequireSignedTokens = false,

                                SignatureValidator = delegate (string token, TokenValidationParameters validationParameters)
                                {
                                    return new JwtSecurityToken(token);
                                },

                                ValidateIssuerSigningKey = false,

                                // Ensure the token hasn't expired:
                                RequireExpirationTime = false,
                                ValidateLifetime = false,

                                ValidateAudience = false,
                                //ValidAudience = tokenConfigurations.oidc.Audience,

                                ValidateIssuer = false,
                                //ValidIssuer = tokenConfigurations.oidc.Issuer
                            };

                            if (jwtConfigureOptions != null)
                                jwtConfigureOptions(options);
                        });

            // Ativa o uso do token como forma de autorizar o acesso
            // a recursos deste projeto
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(JwtBearerDefaults.AuthenticationScheme, new AuthorizationPolicyBuilder()
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