using Sentry;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

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
            int userId = httpContextAccessor.HttpContext.User.TryGetId();
            return new SentryUser()
            {
                Id = userId.ToString(),
                IpAddress = httpContextAccessor.HttpContext.Request.GetRemoteAddress(),
                Username = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
                Email = httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
            };
        }
    }
}