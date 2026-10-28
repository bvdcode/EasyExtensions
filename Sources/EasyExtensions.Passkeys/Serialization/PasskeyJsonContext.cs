// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text.Json.Serialization;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Serialization
{
    [JsonSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        AllowDuplicateProperties = false,
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        MaxDepth = 16,
        Converters = [typeof(Base64UrlConverter)])]
    [JsonSerializable(typeof(RegistrationOptions))]
    [JsonSerializable(typeof(AuthenticationOptions))]
    [JsonSerializable(typeof(RegistrationResponse))]
    [JsonSerializable(typeof(AuthenticationResponse))]
    internal partial class PasskeyJsonContext : JsonSerializerContext
    {
    }
}
