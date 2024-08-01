using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using EasyExtensions.Authorization.Models;
using EasyExtensions.Authorization.Builders;

namespace EasyExtensions.Authorization.Services
{
    internal class JwtTokenProvider(IConfiguration _configuration) : ITokenProvider
    {
        private readonly JwtSettings _jwtSettings = _configuration.GetJwtSettings();

        public string CreateToken(Func<ClaimBuilder, ClaimBuilder>? claimBuilder = null)
        {
            var claims = claimBuilder == null ? [] : claimBuilder(new ClaimBuilder()).Build();
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            string algorithm = _jwtSettings.Key.Length switch
            {
                128 / 8 => SecurityAlgorithms.HmacSha256,
                192 / 8 => SecurityAlgorithms.HmacSha384,
                256 / 8 => SecurityAlgorithms.HmacSha512,
                _ => throw new ArgumentOutOfRangeException(nameof(_jwtSettings), "Key length must be 128, 192 or 256 bits"),
            };
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
            var credentials = new SigningCredentials(securityKey, algorithm);
            var expirationDate = _jwtSettings.LifetimeMinutes.HasValue
                ? DateTime.UtcNow.AddMinutes(_jwtSettings.LifetimeMinutes.Value)
                : DateTime.UtcNow.AddMinutes(30);
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