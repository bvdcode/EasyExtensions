using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using EasyExtensions.AspNetCore.Authorization.Models;

namespace EasyExtensions.AspNetCore.Authorization.Extensions
{
    internal static class ConfigurationExtensions
    {
        internal static JwtSettings GetJwtSettings(this IConfiguration configuration)
        {
            const int defaultLifetimeMinutes = 60;

            var jwtSettings = configuration.GetSection("JwtSettings");
            string? lifetimeMinutesStr = jwtSettings.Exists() 
                ? jwtSettings["LifetimeMinutes"]
                : configuration["JwtLifetimeMinutes"];
            int lifetimeMinutes = int.TryParse(lifetimeMinutesStr, out int parsedLifetimeMinutes)
                ? parsedLifetimeMinutes
                : defaultLifetimeMinutes;
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lifetimeMinutes, nameof(lifetimeMinutes));

            string key = (!jwtSettings.Exists() ? configuration["JwtKey"] : jwtSettings["Key"])
                ?? throw new KeyNotFoundException("JwtSettings.Key or JwtKey is not set");

            string algorithm = key.Length switch
            {
                256 / 8 => SecurityAlgorithms.HmacSha256,
                384 / 8 => SecurityAlgorithms.HmacSha384,
                512 / 8 => SecurityAlgorithms.HmacSha512,
                _ => throw new ArgumentOutOfRangeException(nameof(configuration), "Key length must be 128, 192 or 256 bits (16, 24 or 32 symbols), but was " + key.Length + " symbols.")
            };

            return new JwtSettings
            {
                Key = key,
                Algorithm = algorithm,
                Issuer = (!jwtSettings.Exists() ? configuration["JwtIssuer"] : jwtSettings["Issuer"])
                    ?? throw new KeyNotFoundException("JwtSettings.Issuer or JwtIssuer is not set"),
                Audience = (!jwtSettings.Exists() ? configuration["JwtAudience"] : jwtSettings["Audience"])
                    ?? throw new KeyNotFoundException("JwtSettings.Audience or JwtAudience is not set"),
                LifetimeMinutes = lifetimeMinutes
            };
        }
    }
}
