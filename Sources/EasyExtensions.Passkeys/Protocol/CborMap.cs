// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Cbor;

namespace EasyExtensions.Passkeys.Protocol
{
    internal class CborMap
    {
        private readonly Dictionary<string, ReadOnlyMemory<byte>> values = new(StringComparer.Ordinal);

        internal static CborMap Read(ReadOnlyMemory<byte> encoded, bool integerKeys = false)
        {
            CborReader reader = new(encoded, CborConformanceMode.Strict);
            int? count = reader.ReadStartMap();
            if (count is null || count > 32)
            {
                throw new PasskeyVerificationException("A bounded definite-length CBOR map is required.");
            }
            CborMap map = new();
            for (int index = 0; index < count; index++)
            {
                string key = integerKeys ? reader.ReadInt32().ToString(System.Globalization.CultureInfo.InvariantCulture) : reader.ReadTextString();
                ReadOnlyMemory<byte> value = reader.ReadEncodedValue();
                if (!map.values.TryAdd(key, value))
                {
                    throw new PasskeyVerificationException("Duplicate CBOR map key.");
                }
                Validate(value, 0);
            }
            reader.ReadEndMap();
            if (reader.BytesRemaining != 0)
            {
                throw new PasskeyVerificationException("Trailing CBOR data.");
            }
            return map;
        }

        internal bool Contains(string key) => values.ContainsKey(key);
        internal int Count => values.Count;
        internal ReadOnlyMemory<byte> Get(string key)
        {
            return values.TryGetValue(key, out ReadOnlyMemory<byte> value)
                ? value : throw new PasskeyVerificationException($"Missing CBOR field: {key}.");
        }
        internal byte[] Bytes(string key) => new CborReader(Get(key)).ReadByteString();
        internal string Text(string key) => new CborReader(Get(key)).ReadTextString();
        internal int Integer(string key) => new CborReader(Get(key)).ReadInt32();

        internal byte[][] Certificates()
        {
            CborReader reader = new(Get("x5c"), CborConformanceMode.Strict);
            int? count = reader.ReadStartArray();
            if (count is null or < 1 or > ProtocolLimits.MaximumCertificateCount)
            {
                throw new PasskeyVerificationException("Invalid attestation certificate count.");
            }
            List<byte[]> certificates = [];
            for (int index = 0; index < count; index++)
            {
                byte[] certificate = reader.ReadByteString();
                if (certificate.Length is 0 or > ProtocolLimits.MaximumCertificateLength)
                {
                    throw new PasskeyVerificationException("Invalid attestation certificate size.");
                }
                certificates.Add(certificate);
            }
            reader.ReadEndArray();
            return certificates.ToArray();
        }

        private static void Validate(ReadOnlyMemory<byte> encoded, int depth)
        {
            if (depth > 8)
            {
                throw new PasskeyVerificationException("CBOR nesting limit exceeded.");
            }
            CborReader reader = new(encoded, CborConformanceMode.Strict);
            switch (reader.PeekState())
            {
                case CborReaderState.StartMap:
                    int? pairs = reader.ReadStartMap();
                    if (pairs is null or > 128)
                    {
                        throw new PasskeyVerificationException("Invalid nested CBOR map.");
                    }
                    for (int index = 0; index < pairs * 2; index++)
                    {
                        Validate(reader.ReadEncodedValue(), depth + 1);
                    }
                    reader.ReadEndMap();
                    break;
                case CborReaderState.StartArray:
                    int? count = reader.ReadStartArray();
                    if (count is null or > 128)
                    {
                        throw new PasskeyVerificationException("Invalid CBOR array.");
                    }
                    for (int index = 0; index < count; index++)
                    {
                        Validate(reader.ReadEncodedValue(), depth + 1);
                    }
                    reader.ReadEndArray();
                    break;
                case CborReaderState.Tag:
                case CborReaderState.StartIndefiniteLengthByteString:
                case CborReaderState.StartIndefiniteLengthTextString:
                    throw new PasskeyVerificationException("Unsupported CBOR encoding.");
                default:
                    reader.SkipValue();
                    break;
            }
        }
    }
}
