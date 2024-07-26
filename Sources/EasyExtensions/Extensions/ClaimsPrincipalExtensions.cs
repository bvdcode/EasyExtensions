using System.Security.Claims;
using System.Collections.Generic;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="ClaimsPrincipal"/> extensions.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Get user id.
        /// </summary>
        /// <param name="user"> User instance. </param>
        /// <returns> User id. </returns>
        /// <exception cref="KeyNotFoundException"> Throws when claim not found. </exception>
        public static int GetId(this ClaimsPrincipal? user)
        {
            if (user == null)
            {
                throw new KeyNotFoundException(ClaimTypes.Sid);
            }
            var value = user.FindFirst(ClaimTypes.Sid)
                ?? throw new KeyNotFoundException(ClaimTypes.Sid);
            return int.Parse(value.Value);
        }

        /// <summary>
        /// Try get user id.
        /// </summary>
        /// <param name="user"> User instance. </param>
        /// <returns> User id, or 0 if not found. </returns>
        public static int TryGetId(this ClaimsPrincipal? user)
        {
            if (user == null)
            {
                return 0;
            }
            var value = user.FindFirst(ClaimTypes.Sid);
            return value == null ? 0 : int.Parse(value.Value);
        }

        /// <summary>
        /// Get user roles.
        /// </summary>
        /// <param name="user"> User instance. </param>
        /// <param name="rolePrefix"> Role prefix, for example: "user-group-" prefix returns group like "user-group-admins" </param>
        /// <returns> User roles. </returns>
        public static IEnumerable<string> GetRoles(this ClaimsPrincipal user, string rolePrefix = "")
        {
            if (user == null || user.Claims == null)
            {
                yield break;
            }

            foreach (var item in user.Claims)
            {
                if (item.Type == ClaimsIdentity.DefaultRoleClaimType)
                {
                    if (item.Value.ToLower().StartsWith(rolePrefix))
                    {
                        yield return item.Value;
                    }
                }
            }
        }
    }
}