// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Security.Cryptography;
using System.Text.Json;
using EasyExtensions.Passkeys.Serialization;

namespace EasyExtensions.Passkeys.Protocol
{
    internal static class ClientData
    {
        internal static byte[] Verify(byte[] json, string operation, byte[] challenge, IReadOnlySet<string> origins)
        {
            ArgumentNullException.ThrowIfNull(json);
            if (json.Length is 0 or > ProtocolLimits.MaximumClientDataLength
                || challenge.Length != ProtocolLimits.ChallengeLength)
            {
                throw new PasskeyVerificationException("Invalid client data or challenge length.");
            }
            using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                MaxDepth = 16,
                AllowDuplicateProperties = false
            });
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || RequiredString(root, "type") != operation
                || RequiredString(root, "challenge") != Base64UrlConverter.Encode(challenge)
                || !origins.Contains(RequiredString(root, "origin")))
            {
                throw new PasskeyVerificationException("The operation, challenge or origin does not match.");
            }
            if (root.TryGetProperty("crossOrigin", out JsonElement crossOrigin) && crossOrigin.ValueKind != JsonValueKind.False)
            {
                throw new PasskeyVerificationException("Cross-origin ceremonies are not supported.");
            }
            if (root.TryGetProperty("topOrigin", out _))
            {
                throw new PasskeyVerificationException("Embedded ceremonies are not supported.");
            }
            return SHA256.HashData(json);
        }

        private static string RequiredString(JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.String)
            {
                throw new PasskeyVerificationException($"Missing or invalid client data field: {name}.");
            }
            return value.GetString() ?? throw new PasskeyVerificationException($"Missing client data field: {name}.");
        }
    }
}
