using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="ClaimsPrincipal"/> extensions.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        const string customClaimKey = "tkd";

        /// <summary>
        /// Get TMKID user identifier.
        /// </summary>
        /// <returns> Authorized user identifier. </returns>
        /// <exception cref="KeyNotFoundException"> Claim with key not found. </exception>
        /// <exception cref="FormatException"> Claim with key has incorrect value. </exception>
        /// <exception cref="ArgumentNullException"> User is null. </exception>
        public static int GetTmkId(this ClaimsPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            Claim? claim = user.Claims.FirstOrDefault(x => x.Type == customClaimKey) ?? throw new KeyNotFoundException($"Claim with key '{customClaimKey}' not found.");
            bool parsed = int.TryParse(claim.Value, out int result);
            if (!parsed)
            {
                throw new FormatException($"Claim with key '{customClaimKey}' has incorrect value: '{claim.Value}'.");
            }
            return result;
        }

        /// <summary>
        /// Try get TMKID user identifier.
        /// </summary>
        /// <returns> Authorized user identifier, or -1 if not specified. </returns>
        public static int TryGetTmkId(this ClaimsPrincipal user)
        {
            if (user == null)
            {
                return -1;
            }
            const int defaultUserId = -1;
            if (user == null)
            {
                return defaultUserId;
            }
            Claim? claim = user.Claims.FirstOrDefault(x => x.Type == customClaimKey);
            if (claim == null)
            {
                return defaultUserId;
            }
            bool parsed = int.TryParse(claim.Value, out int result);
            return parsed ? result : defaultUserId;
        }

        /// <summary>
        /// Get user roles.
        /// </summary>
        /// <param name="user"> User instance. </param>
        /// <param name="rolePrefix"> Role prefix, for example: "tccv-group-" prefix returns group like "tccv-group-controllers" </param>
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