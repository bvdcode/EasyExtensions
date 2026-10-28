// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Protocol
{
    internal class ServerConfiguration
    {
        internal PasskeyServerOptions Options { get; }
        internal IReadOnlySet<string> Origins { get; }

        internal ServerConfiguration(PasskeyServerOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.RelyingPartyId);
            ArgumentException.ThrowIfNullOrWhiteSpace(options.RelyingPartyName);
            string id = options.RelyingPartyId;
            if (Uri.CheckHostName(id) != UriHostNameType.Dns || id != id.ToLowerInvariant()
                || id.EndsWith('.') || (id != "localhost" && !id.Contains('.')) || IPAddress.TryParse(id, out _))
            {
                throw new ArgumentException("RP ID must be a lowercase DNS hostname.", nameof(options));
            }
            if (options.TimeoutMilliseconds <= 0 || options.Algorithms.Count == 0 || options.AllowedOrigins.Count == 0)
            {
                throw new ArgumentException("Timeout, algorithms and origins must be configured.", nameof(options));
            }
            foreach (CoseAlgorithm algorithm in options.Algorithms)
            {
                ValidateAlgorithm(algorithm);
            }
            _ = RequiresVerification(options.UserVerification);
            _ = options.ResidentKey switch
            {
                ResidentKeyRequirement.Required or ResidentKeyRequirement.Preferred or ResidentKeyRequirement.Discouraged => true,
                _ => throw new ArgumentException("Invalid resident key requirement.", nameof(options))
            };
            _ = options.Attestation switch
            {
                AttestationPreference.None or AttestationPreference.Direct or AttestationPreference.Indirect or AttestationPreference.Enterprise => true,
                _ => throw new ArgumentException("Invalid attestation preference.", nameof(options))
            };
            HashSet<string> origins = new(StringComparer.Ordinal);
            foreach (string origin in options.AllowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri) || !string.IsNullOrEmpty(uri.UserInfo)
                    || uri.AbsolutePath != "/" || uri.Query.Length != 0 || uri.Fragment.Length != 0
                    || origin != uri.GetLeftPart(UriPartial.Authority)
                    || (uri.Scheme != Uri.UriSchemeHttps && !(uri.Scheme == Uri.UriSchemeHttp && uri.Host == "localhost"))
                    || !(uri.IdnHost == id || uri.IdnHost.EndsWith("." + id, StringComparison.Ordinal)))
                {
                    throw new ArgumentException("Origins must be exact secure origins within the RP ID.", nameof(options));
                }
                origins.Add(origin);
            }
            Origins = origins;
            Options = options with
            {
                AllowedOrigins = origins.ToArray(),
                Algorithms = options.Algorithms.Distinct().ToArray(),
                AttestationTrustRoots = options.AttestationTrustRoots.Select(root => root.ToArray()).ToArray()
            };
        }

        internal static bool RequiresVerification(UserVerificationRequirement requirement)
        {
            return requirement switch
            {
                UserVerificationRequirement.Required => true,
                UserVerificationRequirement.Preferred or UserVerificationRequirement.Discouraged => false,
                _ => throw new PasskeyVerificationException("Invalid user verification requirement.")
            };
        }

        private static void ValidateAlgorithm(CoseAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case CoseAlgorithm.ES256:
                case CoseAlgorithm.ES384:
                case CoseAlgorithm.ES512:
                case CoseAlgorithm.ES256K:
                case CoseAlgorithm.EdDSA:
                case CoseAlgorithm.RS256:
                case CoseAlgorithm.RS384:
                case CoseAlgorithm.RS512:
                case CoseAlgorithm.PS256:
                case CoseAlgorithm.PS384:
                case CoseAlgorithm.PS512:
                    return;
                default:
                    throw new ArgumentException("Unsupported COSE algorithm.", nameof(algorithm));
            }
        }
    }
}
