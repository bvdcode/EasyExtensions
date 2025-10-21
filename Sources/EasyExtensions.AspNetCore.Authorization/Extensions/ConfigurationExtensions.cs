﻿using System;
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
            const int defaultLifetimeMinutes = 15;
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

            int utf8KeyLength = System.Text.Encoding.UTF8.GetByteCount(key);
            string algorithm = utf8KeyLength switch
            {
                256 / 8 => SecurityAlgorithms.HmacSha256,
                384 / 8 => SecurityAlgorithms.HmacSha384,
                512 / 8 => SecurityAlgorithms.HmacSha512,
                _ => throw new ArgumentOutOfRangeException(nameof(configuration), "Key length must be 256, 384 or 512 bits (32, 48 or 64 bytes), but was " + utf8KeyLength + " bytes.")
            };

            return new JwtSettings
            {
                Key = key,
                Algorithm = algorithm,
                Issuer = (!jwtSettings.Exists() ? configuration["JwtIssuer"] : jwtSettings["Issuer"])
                    ?? "EasyExtensions/" + AppDomain.CurrentDomain.FriendlyName,
                Audience = (!jwtSettings.Exists() ? configuration["JwtAudience"] : jwtSettings["Audience"])
                    ?? AppDomain.CurrentDomain.FriendlyName + "/Client",
                LifetimeMinutes = lifetimeMinutes
            };
        }
    }
}
