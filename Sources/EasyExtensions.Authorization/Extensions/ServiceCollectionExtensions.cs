using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using EasyExtensions.Authorization.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EasyExtensions.Authorization.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds CORS policy with origins.
        /// </summary>
        /// <param name="services"> <see cref="IServiceCollection"/> instance. </param>
        /// <param name="policyName"> Name of the policy. </param>
        /// <param name="origins"> Origins to add to the policy. </param>
        /// <returns></returns>
        public static IServiceCollection AddCorsWithOrigins(this IServiceCollection services, string policyName, params string[] origins)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: policyName,
                    policy =>
                    {
                        policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .WithExposedHeaders("*");
                    });
            });
            return services;
        }

        /// <summary>
        /// Adds JWT authentication from JwtSettings section or Jwt[Key] configuration values.
        /// </summary>
        /// <param name="services"> <see cref="IServiceCollection"/> instance. </param>
        /// <param name="configuration"> Configuration from which to get JWT settings. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        /// <exception cref="KeyNotFoundException"> When JwtSettings section is not set. </exception>
        public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");

            string secret = (!jwtSettings.Exists() ? configuration["JwtKey"] : jwtSettings["Key"])
                ?? throw new KeyNotFoundException("JwtSettings.Key or JwtKey is not set");

            string issuer = (!jwtSettings.Exists() ? configuration["JwtIssuer"] : jwtSettings["Issuer"])
                ?? throw new KeyNotFoundException("JwtSettings.Issuer or JwtIssuer is not set");

            string audience = (!jwtSettings.Exists() ? configuration["JwtAudience"] : jwtSettings["Audience"]) 
                ?? throw new KeyNotFoundException("JwtSettings.Audience or JwtAudience is not set");

            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(secret);
                    var key = new SymmetricSecurityKey(bytes);
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = key,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"].ToString();
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            return services;
        }

        /// <summary>
        /// Ignore <see cref="AuthorizeAttribute"/> when application is on development environment.
        /// </summary>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AllowAnonymousOnDevelopment(this IServiceCollection services)
        {
            string currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;
            if (currentEnvironment == "Development")
            {
                var descriptor = new ServiceDescriptor(typeof(IAuthorizationHandler), typeof(AllowAnonymousAuthorizationHandler), ServiceLifetime.Singleton);
                services.Add(descriptor);
            }
            return services;
        }
    }
}