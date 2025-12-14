using System.Collections.Generic;
using System.Security.Claims;

namespace EasyExtensions.AspNetCore.Authorization.Abstractions
{
    /// <summary>
    /// Claim provider.
    /// </summary>
    public interface IClaimProvider
    {
        /// <summary>
        /// Gets the claims.
        /// </summary>
        /// <returns> Claims. </returns>
        IEnumerable<Claim> GetClaims();
    }
}
