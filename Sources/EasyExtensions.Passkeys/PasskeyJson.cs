// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text.Json;
using EasyExtensions.Passkeys.Models;
using EasyExtensions.Passkeys.Serialization;

namespace EasyExtensions.Passkeys
{
    public static class PasskeyJson
    {
        public static string Serialize(RegistrationOptions options)
        {
            return JsonSerializer.Serialize(options, PasskeyJsonContext.Default.RegistrationOptions);
        }

        public static string Serialize(AuthenticationOptions options)
        {
            return JsonSerializer.Serialize(options, PasskeyJsonContext.Default.AuthenticationOptions);
        }

        public static RegistrationResponse ReadRegistration(string json)
        {
            ValidateLength(json);
            return JsonSerializer.Deserialize(json, PasskeyJsonContext.Default.RegistrationResponse)
                ?? throw new JsonException("A registration response is required.");
        }

        public static AuthenticationResponse ReadAuthentication(string json)
        {
            ValidateLength(json);
            return JsonSerializer.Deserialize(json, PasskeyJsonContext.Default.AuthenticationResponse)
                ?? throw new JsonException("An authentication response is required.");
        }

        public static RegistrationOptions ReadRegistrationOptions(string json)
        {
            ValidateLength(json);
            return JsonSerializer.Deserialize(json, PasskeyJsonContext.Default.RegistrationOptions)
                ?? throw new JsonException("Registration options are required.");
        }

        public static AuthenticationOptions ReadAuthenticationOptions(string json)
        {
            ValidateLength(json);
            return JsonSerializer.Deserialize(json, PasskeyJsonContext.Default.AuthenticationOptions)
                ?? throw new JsonException("Authentication options are required.");
        }

        private static void ValidateLength(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            if (json.Length > ProtocolLimits.MaximumEncodedResponseLength)
            {
                throw new JsonException("The response exceeds the size limit.");
            }
            using JsonDocument document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                MaxDepth = 16,
                AllowDuplicateProperties = false
            });
        }
    }
}
