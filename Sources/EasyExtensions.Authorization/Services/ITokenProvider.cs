using System;
using EasyExtensions.Authorization.Builders;

namespace EasyExtensions.Authorization.Services
{
    /// <summary>
    /// Provides token creation with claims.
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// Creates a token with claims.
        /// </summary>
        /// <param name="claimBuilder"> Optional claim builder. </param>
        /// <returns> Token. </returns>
        string CreateToken(Func<ClaimBuilder, ClaimBuilder>? claimBuilder = null);
    }
}