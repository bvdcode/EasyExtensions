using System;
using EasyExtensions.AspNetCore.Authorization.Builders;
using EasyExtensions.AspNetCore.Authorization.Abstractions;

namespace EasyExtensions.AspNetCore.Authorization.Services
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

        /// <summary>
        /// Creates a token with claims.
        /// </summary>
        /// <param name="claimProvider"> Claim provider. </param>
        /// <returns> Token. </returns>
        string CreateToken(IClaimProvider claimProvider);
    }
}