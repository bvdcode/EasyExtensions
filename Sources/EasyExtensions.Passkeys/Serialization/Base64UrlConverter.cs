// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyExtensions.Passkeys.Serialization
{
    public class Base64UrlConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString() ?? throw new JsonException("A base64url string is required.");
            if (value.Length > ProtocolLimits.MaximumEncodedResponseLength)
            {
                throw new JsonException("The binary value exceeds the size limit.");
            }
            if (value.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-' && character != '_')
                || !Base64Url.IsValid(value))
            {
                throw new JsonException("Invalid unpadded base64url value.");
            }
            byte[] bytes = Base64Url.DecodeFromChars(value);
            if (Encode(bytes) != value)
            {
                throw new JsonException("Noncanonical base64url value.");
            }
            return bytes;
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Encode(value));
        }

        public static string Encode(ReadOnlySpan<byte> bytes)
        {
            return Base64Url.EncodeToString(bytes);
        }
    }
}
