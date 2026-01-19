// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EasyExtensions
{
    /// <summary>
    /// <see cref="ClaimsPrincipal"/> extensions.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Retrieves the user name or unique identifier from the specified claims principal.
        /// </summary>
        /// <remarks>This method attempts to retrieve the user's name by first checking for a name claim
        /// (<see cref="System.Security.Claims.ClaimTypes.Name"/>), then a subject claim ('sub'), and finally a name
        /// identifier claim (<see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>). If none of these claims
        /// are found, an exception is thrown.</remarks>
        /// <param name="user">The claims principal from which to extract the user name. Cannot be null.</param>
        /// <returns>A string containing the user's name, subject identifier ('sub'), or name identifier claim value, in that
        /// order of precedence.</returns>
        /// <exception cref="NullReferenceException">Thrown if the <paramref name="user"/> parameter is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if neither a name, subject ('sub'), nor name identifier claim is present in the claims principal.</exception>
        public static string GetUserName(this ClaimsPrincipal? user)
        {
            if (user == null)
            {
                throw new NullReferenceException(nameof(user));
            }
            Claim? name = user.FindFirst(ClaimTypes.Name);
            if (name != null)
            {
                return name.Value;
            }
            Claim? sub = user.FindFirst("sub");
            if (sub != null)
            {
                return sub.Value;
            }

            Claim? nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifier != null)
            {
                return nameIdentifier.Value;
            }

            throw new KeyNotFoundException("'sub' or " + ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Attempts to extract the user's unique identifier from the specified claims principal.
        /// </summary>
        /// <remarks>This method searches for a claim with the type "sub" or <see
        /// cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>. If either claim is present and contains a valid
        /// GUID value, that value is returned in <paramref name="userId"/>.</remarks>
        /// <param name="user">The claims principal from which to retrieve the user identifier. May be null.</param>
        /// <param name="userId">When this method returns, contains the user identifier as a <see cref="Guid"/> if found; otherwise, <see
        /// cref="Guid.Empty"/>.</param>
        /// <returns>true if a user identifier was successfully retrieved; otherwise, false.</returns>
        public static bool TryGetUserId(this ClaimsPrincipal? user, out Guid userId)
        {
            userId = Guid.Empty;
            if (user == null)
            {
                return false;
            }
            Claim? sub = user.FindFirst("sub");
            if (sub != null)
            {
                userId = Guid.Parse(sub.Value);
                return true;
            }
            Claim? nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifier != null)
            {
                userId = Guid.Parse(nameIdentifier.Value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retrieves the unique identifier (GUID) for the current user from the specified claims principal.
        /// </summary>
        /// <remarks>This method attempts to locate the user's unique identifier by first searching for
        /// the 'sub' claim, which is commonly used in OpenID Connect and OAuth 2.0 scenarios. If the 'sub' claim is not
        /// found, it falls back to the NameIdentifier claim. The method expects the claim value to be a valid GUID
        /// string.</remarks>
        /// <param name="user">The claims principal representing the authenticated user. Cannot be null.</param>
        /// <returns>The GUID value of the user's unique identifier, extracted from the 'sub' claim if present; otherwise, from
        /// the NameIdentifier claim.</returns>
        /// <exception cref="NullReferenceException">Thrown if <paramref name="user"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if neither the 'sub' claim nor the NameIdentifier claim is present in <paramref name="user"/>.</exception>
        public static Guid GetUserId(this ClaimsPrincipal? user)
        {
            if (user == null)
            {
                throw new NullReferenceException(nameof(user));
            }
            Claim? sub = user.FindFirst("sub");
            if (sub != null)
            {
                return Guid.Parse(sub.Value);
            }

            Claim? nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifier != null)
            {
                return Guid.Parse(nameIdentifier.Value);
            }

            throw new KeyNotFoundException("'sub' or " + ClaimTypes.NameIdentifier);
        }

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
