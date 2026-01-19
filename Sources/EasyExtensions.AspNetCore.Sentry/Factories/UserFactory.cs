// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Sentry;
using System;
using System.Linq;
using System.Security.Claims;

namespace EasyExtensions.AspNetCore.Sentry.Factories
{
    /// <summary>
    /// Implementaion of <see cref="ISentryUserFactory"/> with JWT payload parsing.
    /// </summary>
    public class UserFactory(IHttpContextAccessor httpContextAccessor) : ISentryUserFactory
    {
        /// <summary>
        /// Create <see cref="SentryUser"/> from JWT payload information.
        /// </summary>
        /// <returns> <see cref="SentryUser"/> instance. </returns>
        public SentryUser? Create()
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return null;
            }
            var context = httpContextAccessor.HttpContext;
            if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                return null;
            }
            var claims = httpContextAccessor.HttpContext.User.Claims;
            int userId = httpContextAccessor.HttpContext.User.TryGetId();
            bool hasUserId = httpContextAccessor.HttpContext.User.TryGetUserId(out Guid guidUserId);
            string userIdStr = hasUserId ? guidUserId.ToString() : userId > 0 ? userId.ToString() : "Anonymous";
            string username = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value
                ?? claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value
                ?? "Anonymous";
            string email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value
                ?? claims.FirstOrDefault(x => x.Type == "email")?.Value
                ?? string.Empty;

            return new SentryUser()
            {
                Id = userIdStr,
                IpAddress = context.Request.GetRemoteAddress(),
                Username = username,
                Email = email,
            };
        }
    }
}
