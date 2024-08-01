using System;
using System.Security.Claims;
using System.Collections.Generic;

namespace EasyExtensions.Authorization.Builders
{
    /// <summary>
    /// Claim builder.
    /// </summary>
    public class ClaimBuilder
    {
        private readonly List<Claim> _claims = [];

        /// <summary>
        /// Adds a claim to the builder.
        /// </summary>
        /// <param name="type"> Claim type. </param>
        /// <param name="value"> Claim value. </param>
        /// <returns> Current <see cref="ClaimBuilder"/> instance. </returns>
        /// <exception cref="ArgumentException"> When <paramref name="type"/> or <paramref name="value"/> is null or empty. </exception>
        public ClaimBuilder Add(string type, string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(type, nameof(type));
            ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));
            _claims.Add(new Claim(type, value));
            return this;
        }

        /// <summary>
        /// Adds a claim to the builder.
        /// </summary>
        /// <param name="claim"> Claim to add. </param>
        /// <returns> Current <see cref="ClaimBuilder"/> instance. </returns>
        /// <exception cref="ArgumentNullException"> When <paramref name="claim"/> is null or <see cref="Claim.Type"/> or <see cref="Claim.Value"/> is empty. </exception>
        public ClaimBuilder Add(Claim claim)
        {
            ArgumentNullException.ThrowIfNull(claim, nameof(claim));
            ArgumentNullException.ThrowIfNull(claim.Type, nameof(claim.Type));
            ArgumentNullException.ThrowIfNull(claim.Value, nameof(claim.Value));
            _claims.Add(claim);
            return this;
        }

        /// <summary>
        /// Builds the claims.
        /// </summary>
        /// <returns> Array of claims. </returns>
        public Claim[] Build()
        {
            return [.. _claims];
        }
    }
}