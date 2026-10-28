// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Binary;
using System.Formats.Asn1;
using System.Formats.Cbor;
using System.Security.Cryptography;

namespace EasyExtensions.Passkeys.Tests
{
    internal static class TpmFixture
    {
        internal static byte[] Subject()
        {
            AsnWriter writer = new(AsnEncodingRules.DER);
            writer.PushSequence();
            Asn1Tag tag = new(TagClass.ContextSpecific, 4, isConstructed: true);
            writer.PushSequence(tag);
            writer.PushSequence();
            foreach ((string oid, string value) in new[] { ("2.23.133.2.1", "id:49465800"), ("2.23.133.2.2", "Model"), ("2.23.133.2.3", "1.0") })
            {
                writer.PushSetOf();
                writer.PushSequence();
                writer.WriteObjectIdentifier(oid);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, value);
                writer.PopSequence();
                writer.PopSetOf();
            }
            writer.PopSequence();
            writer.PopSequence(tag);
            writer.PopSequence();
            return writer.Encode();
        }

        internal static byte[] Statement(BrowserCredential browser, byte[] signed, byte[] certificate, bool wrongBinding)
        {
            ECParameters key = browser.EcParameters();
            using MemoryStream area = new();
            area.Write(Convert.FromHexString("0023000B0004007200000010001000030010"));
            Sized(area, key.Q.X!);
            Sized(area, key.Q.Y!);
            byte[] publicArea = area.ToArray();
            using MemoryStream info = new();
            info.Write(Convert.FromHexString("FF54434780170000"));
            byte[] digest = SHA256.HashData(signed);
            if (wrongBinding)
            {
                digest[0] ^= 1;
            }
            Sized(info, digest);
            info.Write(new byte[25]);
            Sized(info, [0, 0x0B, .. SHA256.HashData(publicArea)]);
            Sized(info, []);
            byte[] certInfo = info.ToArray();
            CborWriter writer = new();
            writer.WriteStartMap(6);
            writer.WriteTextString("ver");
            writer.WriteTextString("2.0");
            writer.WriteTextString("alg");
            writer.WriteInt32((int)browser.Algorithm);
            writer.WriteTextString("pubArea");
            writer.WriteByteString(publicArea);
            writer.WriteTextString("certInfo");
            writer.WriteByteString(certInfo);
            writer.WriteTextString("sig");
            writer.WriteByteString(browser.Sign(certInfo));
            AttestationFixture.Certificates(writer, certificate);
            writer.WriteEndMap();
            return writer.Encode();
        }

        private static void Sized(Stream stream, byte[] data)
        {
            Span<byte> length = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(length, (ushort)data.Length);
            stream.Write(length);
            stream.Write(data);
        }
    }
}
