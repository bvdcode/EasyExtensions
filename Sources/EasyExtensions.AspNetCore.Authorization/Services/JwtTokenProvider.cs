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

        public string CreateToken(IClaimProvider claimProvider)
        {
            return CreateToken(x => x.AddRange(claimProvider.GetClaims()));
        }

        public string CreateToken(Func<ClaimBuilder, ClaimBuilder>? claimBuilder = null)
        {
            var claims = claimBuilder == null ? [] : claimBuilder(new ClaimBuilder()).Build();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, _jwtSettings.Algorithm);
            var expirationDate = _jwtSettings.LifetimeMinutes.HasValue
                ? DateTime.UtcNow.AddMinutes(_jwtSettings.LifetimeMinutes.Value)
                : DateTime.UtcNow.AddMinutes(defaultLifetimeMinutes);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expirationDate,
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler()
                .WriteToken(tokenDescriptor);
        }
    }
}