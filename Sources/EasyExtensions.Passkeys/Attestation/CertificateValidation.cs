// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EasyExtensions.Passkeys.Cryptography;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Attestation
{
    internal static class CertificateValidation
    {
        internal static bool VerifySignature(X509Certificate2 certificate, CoseAlgorithm algorithm, byte[] data, byte[] signature)
        {
            switch (algorithm)
            {
                case CoseAlgorithm.ES256:
                case CoseAlgorithm.ES384:
                case CoseAlgorithm.ES512:
                case CoseAlgorithm.ES256K:
                    using (ECDsa? key = certificate.GetECDsaPublicKey())
                    {
                        if (key is null)
                        {
                            return false;
                        }
                        string? oid = key.ExportParameters(false).Curve.Oid.Value;
                        bool correctCurve = (algorithm, oid) switch
                        {
                            (CoseAlgorithm.ES256, "1.2.840.10045.3.1.7") => true,
                            (CoseAlgorithm.ES384, "1.3.132.0.34") => true,
                            (CoseAlgorithm.ES512, "1.3.132.0.35") => true,
                            (CoseAlgorithm.ES256K, "1.3.132.0.10") => true,
                            _ => false
                        };
                        return correctCurve && key.VerifyData(data, signature, CoseKey.HashName(algorithm), DSASignatureFormat.Rfc3279DerSequence);
                    }
                case CoseAlgorithm.RS256:
                case CoseAlgorithm.RS384:
                case CoseAlgorithm.RS512:
                case CoseAlgorithm.PS256:
                case CoseAlgorithm.PS384:
                case CoseAlgorithm.PS512:
                    using (RSA? key = certificate.GetRSAPublicKey())
                    {
                        return key is not null && key.KeySize >= 2048 && key.VerifyData(data, signature, CoseKey.HashName(algorithm), CoseKey.RsaPadding(algorithm));
                    }
                case CoseAlgorithm.EdDSA:
                    return certificate.PublicKey.Oid.Value == "1.3.101.112"
                        && Ed25519Signature.Verify(certificate.PublicKey.EncodedKeyValue.RawData, data, signature);
                default:
                    throw new PasskeyVerificationException("Unsupported attestation signature algorithm.");
            }
        }

        internal static bool MatchesKey(X509Certificate2 certificate, CoseKey key)
        {
            switch (key.Type)
            {
                case 2:
                    using (ECDsa? certificateKey = certificate.GetECDsaPublicKey())
                    using (ECDsa credentialKey = key.CreateECDsa())
                    {
                        return certificateKey is not null
                            && certificateKey.ExportSubjectPublicKeyInfo().AsSpan().SequenceEqual(credentialKey.ExportSubjectPublicKeyInfo());
                    }
                case 3:
                    using (RSA? certificateKey = certificate.GetRSAPublicKey())
                    {
                        if (certificateKey is null)
                        {
                            return false;
                        }
                        RSAParameters parameters = certificateKey.ExportParameters(false);
                        return key.Modulus.AsSpan().SequenceEqual(parameters.Modulus) && key.Exponent.AsSpan().SequenceEqual(parameters.Exponent);
                    }
                case 1:
                    return certificate.PublicKey.Oid.Value == "1.3.101.112"
                        && key.X.AsSpan().SequenceEqual(certificate.PublicKey.EncodedKeyValue.RawData);
                default:
                    throw new PasskeyVerificationException("Unsupported certificate public key.");
            }
        }

        internal static AttestationTrust Trust(byte[][] encoded, PasskeyServerOptions options)
        {
            List<X509Certificate2> certificates = [];
            List<X509Certificate2> roots = [];
            try
            {
                foreach (byte[] certificate in encoded)
                {
                    certificates.Add(X509CertificateLoader.LoadCertificate(certificate));
                }
                foreach (byte[] root in options.AttestationTrustRoots)
                {
                    roots.Add(X509CertificateLoader.LoadCertificate(root));
                }
                DateTime now = DateTime.UtcNow;
                if (certificates.Any(certificate => now < certificate.NotBefore.ToUniversalTime() || now > certificate.NotAfter.ToUniversalTime()))
                {
                    throw new PasskeyVerificationException("The attestation certificate is outside its validity period.");
                }
                if (roots.Count == 0)
                {
                    return AttestationTrust.Untrusted;
                }
                using X509Chain chain = new();
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.AddRange(roots.ToArray());
                chain.ChainPolicy.ExtraStore.AddRange(certificates.Skip(1).ToArray());
                chain.ChainPolicy.DisableCertificateDownloads = true;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                return chain.Build(certificates[0]) ? AttestationTrust.Trusted : AttestationTrust.Untrusted;
            }
            finally
            {
                foreach (X509Certificate2 certificate in certificates.Concat(roots))
                {
                    certificate.Dispose();
                }
            }
        }

        internal static void CheckLeaf(X509Certificate2 certificate, Guid aaGuid)
        {
            X509BasicConstraintsExtension? constraints = certificate.Extensions.OfType<X509BasicConstraintsExtension>().SingleOrDefault();
            X509KeyUsageExtension? keyUsage = certificate.Extensions.OfType<X509KeyUsageExtension>().SingleOrDefault();
            if (certificate.Version != 3 || constraints is null || constraints.CertificateAuthority
                || (keyUsage is not null && (keyUsage.KeyUsages & X509KeyUsageFlags.DigitalSignature) == 0))
            {
                throw new PasskeyVerificationException("Invalid attestation certificate constraints.");
            }
            X509Extension? extension = certificate.Extensions["1.3.6.1.4.1.45724.1.1.4"];
            if (extension is not null)
            {
                AsnReader reader = new(extension.RawData, AsnEncodingRules.DER);
                byte[] value = reader.ReadOctetString();
                reader.ThrowIfNotEmpty();
                if (extension.Critical || value.Length != 16 || new Guid(value, bigEndian: true) != aaGuid)
                {
                    throw new PasskeyVerificationException("The certificate AAGUID does not match.");
                }
            }
        }

        internal static Dictionary<string, string> ReadName(ReadOnlyMemory<byte> encoded)
        {
            AsnReader reader = new(encoded, AsnEncodingRules.DER);
            AsnReader sequence = reader.ReadSequence();
            Dictionary<string, string> attributes = new(StringComparer.Ordinal);
            while (sequence.HasData)
            {
                AsnReader set = sequence.ReadSetOf();
                while (set.HasData)
                {
                    AsnReader attribute = set.ReadSequence();
                    string oid = attribute.ReadObjectIdentifier();
                    Asn1Tag tag = attribute.PeekTag();
                    string value = attribute.ReadCharacterString((UniversalTagNumber)tag.TagValue);
                    attribute.ThrowIfNotEmpty();
                    if (!attributes.TryAdd(oid, value))
                    {
                        throw new PasskeyVerificationException("Duplicate certificate subject attribute.");
                    }
                }
            }
            reader.ThrowIfNotEmpty();
            return attributes;
        }
    }
}
