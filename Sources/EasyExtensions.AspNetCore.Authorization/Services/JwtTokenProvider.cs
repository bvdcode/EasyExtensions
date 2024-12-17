using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using EasyExtensions.AspNetCore.Authorization.Models;
using EasyExtensions.AspNetCore.Authorization.Builders;
using EasyExtensions.AspNetCore.Authorization.Extensions;
using EasyExtensions.AspNetCore.Authorization.Abstractions;

namespace EasyExtensions.AspNetCore.Authorization.Services
{
    internal class JwtTokenProvider(IConfiguration _configuration) : ITokenProvider
    {
        private const int defaultLifetimeMinutes = 30;
        private readonly JwtSettings _jwtSettings = _configuration.GetJwtSettings();
        private readonly SymmetricSecurityKey _securityKey = new(Encoding.UTF8.GetBytes(_configuration.GetJwtSettings().Key));

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = _securityKey
            };
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string CreateToken(TimeSpan lifetime, Func<ClaimBuilder, ClaimBuilder>? claimBuilder = null)
        {
            var claims = claimBuilder == null ? [] : claimBuilder(new ClaimBuilder()).Build();
            var credentials = new SigningCredentials(_securityKey, _jwtSettings.Algorithm);
            var expirationDate = DateTime.UtcNow.Add(lifetime);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expirationDate,
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler()
                .WriteToken(tokenDescriptor);
        }

        public string CreateToken(IClaimProvider claimProvider)
        {
            return CreateToken(x => x.AddRange(claimProvider.GetClaims()));
        }

        public string CreateToken(Func<ClaimBuilder, ClaimBuilder>? claimBuilder = null)
        {
            TimeSpan lifetime = _jwtSettings.LifetimeMinutes.HasValue
                ? TimeSpan.FromMinutes(_jwtSettings.LifetimeMinutes.Value)
                : TimeSpan.FromMinutes(defaultLifetimeMinutes);
            return CreateToken(lifetime, claimBuilder);
        }
    }
}