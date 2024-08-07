using System.Linq;
using Sentry.Protocol;
using Sentry.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EasyExtensions.AspNetCore.Sentry.Factories
{
    /// <summary>
    /// Implementaion of <see cref="IUserFactory"/> with JWT payload parsing.
    /// </summary>
    public class UserFactory : IUserFactory
    {
        /// <summary>
        /// Create <see cref="User"/> from JWT payload information.
        /// </summary>
        /// <param name="context"> Current <see cref="HttpContext"/> instance. </param>
        /// <returns> <see cref="User"/> instance. </returns>
        public User Create(HttpContext context)
        {
            int userId = context.User.TryGetId();
            return new User()
            {
                Id = userId.ToString(),
                IpAddress = context.Request.GetRemoteAddress(),
                Username = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value,
                Email = context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value,
            };
        }
    }
}