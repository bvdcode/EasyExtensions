// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Binary;
using System.Formats.Asn1;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EasyExtensions.Passkeys.Cryptography;
using EasyExtensions.Passkeys.Models;
using EasyExtensions.Passkeys.Protocol;

namespace EasyExtensions.Passkeys.Attestation
{
    internal static class TpmAttestation
    {
        internal static AttestationTrust Verify(CborMap statement, AttestationContext context, PasskeyServerOptions options)
        {
            if (statement.Count != 6 || statement.Text("ver") != "2.0" || statement.Contains("ecdaaKeyId"))
            {
                throw new PasskeyVerificationException("Only TPM 2.0 certificate attestation is supported.");
            }
            byte[] publicArea = statement.Bytes("pubArea");
            ushort nameAlgorithm = VerifyPublicArea(publicArea, context.Key);
            CoseAlgorithm algorithm = (CoseAlgorithm)statement.Integer("alg");
            byte[] certificateInfo = statement.Bytes("certInfo");
            VerifyCertificateInfo(certificateInfo, publicArea, nameAlgorithm, algorithm, context.SignedData);
            byte[][] certificates = statement.Certificates();
            using X509Certificate2 leaf = X509CertificateLoader.LoadCertificate(certificates[0]);
            CertificateValidation.CheckLeaf(leaf, context.Credential.AaGuid);
            VerifyCertificate(leaf);
            if (!CertificateValidation.VerifySignature(leaf, algorithm, certificateInfo, statement.Bytes("sig")))
            {
                throw new PasskeyVerificationException("Invalid TPM attestation signature.");
            }
            return CertificateValidation.Trust(certificates, options);
        }

        private static ushort VerifyPublicArea(byte[] encoded, CoseKey key)
        {
            TpmReader reader = new(encoded);
            ushort type = reader.UInt16();
            ushort nameAlgorithm = reader.UInt16();
            _ = reader.UInt32();
            _ = reader.SizedBytes();
            reader.Symmetric();
            reader.Scheme();
            switch (type)
            {
                case 0x0001:
                    ushort bits = reader.UInt16();
                    uint exponent = reader.UInt32();
                    if (exponent == 0)
                    {
                        exponent = 65537;
                    }
                    byte[] modulus = reader.SizedBytes();
                    if (key.Type != 3 || bits != key.Modulus.Length * 8 || !key.Modulus.AsSpan().SequenceEqual(modulus)
                        || new BigInteger(key.Exponent, isUnsigned: true, isBigEndian: true) != exponent)
                    {
                        throw new PasskeyVerificationException("TPM RSA public area does not match the credential key.");
                    }
                    break;
                case 0x0023:
                    ushort curve = reader.UInt16();
                    reader.Scheme();
                    byte[] x = reader.SizedBytes();
                    byte[] y = reader.SizedBytes();
                    bool matchingCurve = (curve, key.Curve) switch
                    {
                        (0x0003, 1) or (0x0004, 2) or (0x0005, 3) => true,
                        _ => false
                    };
                    if (key.Type != 2 || !matchingCurve || !MatchingCoordinate(x, key.X) || !MatchingCoordinate(y, key.Y))
                    {
                        throw new PasskeyVerificationException("TPM EC public area does not match the credential key.");
                    }
                    break;
                default:
                    throw new PasskeyVerificationException("Unsupported TPM public area type.");
            }
            reader.End();
            return nameAlgorithm;
        }

        private static bool MatchingCoordinate(byte[] actual, byte[] expected)
        {
            return actual.Length > 0 && actual.Length <= expected.Length
                && new BigInteger(actual, isUnsigned: true, isBigEndian: true) == new BigInteger(expected, isUnsigned: true, isBigEndian: true);
        }

        private static void VerifyCertificateInfo(byte[] encoded, byte[] publicArea, ushort nameAlgorithm, CoseAlgorithm algorithm, byte[] signedData)
        {
            TpmReader reader = new(encoded);
            if (reader.UInt32() != 0xff544347 || reader.UInt16() != 0x8017)
            {
                throw new PasskeyVerificationException("Invalid TPM certificate magic or type.");
            }
            _ = reader.SizedBytes();
            byte[] extraData = reader.SizedBytes();
            _ = reader.Read(25);
            byte[] name = reader.SizedBytes();
            _ = reader.SizedBytes();
            reader.End();
            HashAlgorithmName nameHash = nameAlgorithm switch
            {
                0x000B => HashAlgorithmName.SHA256,
                0x000C => HashAlgorithmName.SHA384,
                0x000D => HashAlgorithmName.SHA512,
                _ => throw new PasskeyVerificationException("Unsupported TPM name hash algorithm.")
            };
            byte[] digest = CryptographicOperations.HashData(nameHash, publicArea);
            if (!CryptographicOperations.FixedTimeEquals(extraData, CryptographicOperations.HashData(CoseKey.HashName(algorithm), signedData))
                || name.Length != 2 + digest.Length || BinaryPrimitives.ReadUInt16BigEndian(name) != nameAlgorithm
                || !CryptographicOperations.FixedTimeEquals(name.AsSpan(2), digest))
            {
                throw new PasskeyVerificationException("TPM certificate information does not bind the credential and challenge.");
            }
        }

        private static void VerifyCertificate(X509Certificate2 certificate)
        {
            X509EnhancedKeyUsageExtension? usage = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().SingleOrDefault();
            bool hasAikUsage = usage is not null && usage.EnhancedKeyUsages.Cast<Oid>().Any(oid => oid.Value == "2.23.133.8.3");
            if (!certificate.SubjectName.RawData.AsSpan().SequenceEqual(new byte[] { 0x30, 0x00 }) || !hasAikUsage)
            {
                throw new PasskeyVerificationException("Invalid TPM attestation certificate subject or usage.");
            }
            X509Extension extension = certificate.Extensions["2.5.29.17"]
                ?? throw new PasskeyVerificationException("Missing TPM certificate subject alternative name.");
            AsnReader reader = new(extension.RawData, AsnEncodingRules.DER);
            AsnReader names = reader.ReadSequence();
            bool found = false;
            while (names.HasData)
            {
                Asn1Tag tag = names.PeekTag();
                if (tag.HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 4)))
                {
                    AsnReader name = names.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 4, isConstructed: true));
                    Dictionary<string, string> attributes = CertificateValidation.ReadName(name.ReadEncodedValue());
                    name.ThrowIfNotEmpty();
                    if (attributes.TryGetValue("2.23.133.2.1", out string? manufacturer)
                        && manufacturer.Length == 11 && manufacturer.StartsWith("id:", StringComparison.Ordinal)
                        && manufacturer.AsSpan(3).IndexOfAnyExcept("0123456789abcdefABCDEF") < 0
                        && attributes.TryGetValue("2.23.133.2.2", out string? model) && !string.IsNullOrWhiteSpace(model)
                        && attributes.TryGetValue("2.23.133.2.3", out string? version) && !string.IsNullOrWhiteSpace(version))
                    {
                        found = true;
                    }
                }
                else
                {
                    _ = names.ReadEncodedValue();
                }
            }
            reader.ThrowIfNotEmpty();
            if (!found)
            {
                throw new PasskeyVerificationException("Missing TPM manufacturer, model or version.");
            }
        }
    }
}
