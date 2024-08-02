using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using EasyExtensions.Authorization.Models;

namespace EasyExtensions.Authorization.Services
{
    internal static class ConfigurationExtensions
    {
        internal static JwtSettings GetJwtSettings(this IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            int lifetimeMinutes = jwtSettings.Exists() ? int.Parse(jwtSettings["LifetimeMinutes"]!) : int.Parse(configuration["JwtLifetimeMinutes"]!);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lifetimeMinutes, nameof(lifetimeMinutes));

            return new JwtSettings
            {
                Key = (!jwtSettings.Exists() ? configuration["JwtKey"] : jwtSettings["Key"])
                    ?? throw new KeyNotFoundException("JwtSettings.Key or JwtKey is not set"),
                Issuer = (!jwtSettings.Exists() ? configuration["JwtIssuer"] : jwtSettings["Issuer"])
                    ?? throw new KeyNotFoundException("JwtSettings.Issuer or JwtIssuer is not set"),
                Audience = (!jwtSettings.Exists() ? configuration["JwtAudience"] : jwtSettings["Audience"])
                    ?? throw new KeyNotFoundException("JwtSettings.Audience or JwtAudience is not set"),
                LifetimeMinutes = lifetimeMinutes
            };
        }
    }
}
