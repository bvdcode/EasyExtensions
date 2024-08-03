using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
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

            string key = (!jwtSettings.Exists() ? configuration["JwtKey"] : jwtSettings["Key"])
                ?? throw new KeyNotFoundException("JwtSettings.Key or JwtKey is not set");
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            string algorithm = key.Length switch
            {
                256 / 8 => SecurityAlgorithms.HmacSha256,
                384 / 8 => SecurityAlgorithms.HmacSha384,
                512 / 8 => SecurityAlgorithms.HmacSha512,
                _ => throw new ArgumentOutOfRangeException(nameof(key), "Key length must be 128, 192 or 256 bits (16, 24 or 32 symbols)")
            };
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

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
