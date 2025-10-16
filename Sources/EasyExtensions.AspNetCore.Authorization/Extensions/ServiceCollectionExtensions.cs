using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EasyExtensions.AspNetCore.Authorization.Handlers;
using EasyExtensions.AspNetCore.Authorization.Services;
using EasyExtensions.AspNetCore.Authorization.Abstractions;

namespace EasyExtensions.AspNetCore.Authorization.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds JWT authentication resolving <see cref="IConfiguration"/> from DI.
        /// Reads settings from JwtSettings section or flat fallback Jwt[Key] configuration values (see <see cref="ConfigurationExtensions.GetJwtSettings"/>).
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
        /// <returns>Current <see cref="IServiceCollection"/> instance.</returns>
        /// <exception cref="KeyNotFoundException">When required JWT settings are missing.</exception>
        /// <remarks>
        /// <para><b>Configuration keys</b> (precedence: section values override flat values if section exists):</para>
        ///
        /// <para><b>Section-based (preferred):</b></para>
        /// <list type="bullet">
        ///   <item>
        ///     <term><c>JwtSettings:Key</c> (required)</term>
        ///     <description>Symmetric key; length 16/24/32 chars → HMAC SHA-256/384/512 is selected automatically.</description>
        ///   </item>
        ///   <item><term><c>JwtSettings:Issuer</c> (required)</term><description>Token issuer.</description></item>
        ///   <item><term><c>JwtSettings:Audience</c> (required)</term><description>Valid audience.</description></item>
        ///   <item><term><c>JwtSettings:LifetimeMinutes</c> (optional)</term><description>Default 60, must be > 0.</description></item>
        /// </list>
        ///
        /// <para><b>Flat fallback</b> (used only if section <c>JwtSettings</c> does not exist):</para>
        /// <list type="bullet">
        ///   <item><term><c>Jwt[Key]</c> (required)</term><description>The same as in section.</description></item>
        /// </list>
        ///
        /// <para><b>Validation behavior:</b></para>
        /// <list type="bullet">
        ///   <item><description>Issuer, Audience, Lifetime and Signing Key are validated.</description></item>
        ///   <item><description>Clock skew: 5 minutes.</description></item>
        ///   <item><description>Key length determines <see cref="Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256"/>,
        ///   <see cref="Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha384"/>, or
        ///   <see cref="Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha512"/>.</description></item>
        /// </list>
        ///
        /// <para><b>Additional behavior:</b></para>
        /// <list type="bullet">
        ///   <item><description><c>access_token</c> query string (e.g., for WebSockets) is accepted as bearer token.</description></item>
        ///   <item><description><c>RequireHttpsMetadata = false</c> (adjust in production if needed).</description></item>
        /// </list>
        /// </remarks>
        public static IServiceCollection AddJwt(this IServiceCollection services)
        {
            services.AddScoped<ITokenProvider, JwtTokenProvider>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(); // Options configured via IConfigureOptions below

            services.AddSingleton<IConfigureOptions<JwtBearerOptions>>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return new ConfigureNamedOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    var jwtSettings = configuration.GetJwtSettings();
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key);
                    var key = new SymmetricSecurityKey(bytes);
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
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
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
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