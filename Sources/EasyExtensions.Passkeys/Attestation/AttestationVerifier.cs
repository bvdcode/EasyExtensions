// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EasyExtensions.Passkeys.Models;
using EasyExtensions.Passkeys.Protocol;

namespace EasyExtensions.Passkeys.Attestation
{
    internal static class AttestationVerifier
    {
        internal static AttestationTrust Verify(string format, CborMap statement, AttestationContext context, PasskeyServerOptions options)
        {
            AttestationTrust trust = format switch
            {
                "none" => VerifyNone(statement),
                "packed" => VerifyPacked(statement, context, options),
                "fido-u2f" => VerifyU2f(statement, context, options),
                "apple" => VerifyApple(statement, context, options),
                "android-key" => AndroidKeyAttestation.Verify(statement, context, options),
                "tpm" => TpmAttestation.Verify(statement, context, options),
                _ => throw new PasskeyVerificationException($"Unsupported attestation format: {format}.")
            };
            if (options.RequireTrustedAttestation && trust != AttestationTrust.Trusted)
            {
                throw new PasskeyVerificationException("Attestation must chain to a configured trusted root.");
            }
            return trust;
        }

        private static AttestationTrust VerifyNone(CborMap statement)
        {
            if (statement.Count != 0)
            {
                throw new PasskeyVerificationException("None attestation must have an empty statement.");
            }
            return AttestationTrust.None;
        }

        private static AttestationTrust VerifyPacked(CborMap statement, AttestationContext context, PasskeyServerOptions options)
        {
            CoseAlgorithm algorithm = (CoseAlgorithm)statement.Integer("alg");
            byte[] signature = statement.Bytes("sig");
            if (statement.Contains("ecdaaKeyId"))
            {
                throw new PasskeyVerificationException("ECDAA attestation is not supported.");
            }
            if (!statement.Contains("x5c"))
            {
                if (statement.Count != 2 || algorithm != context.Key.Algorithm || !context.Key.Verify(context.SignedData, signature))
                {
                    throw new PasskeyVerificationException("Invalid packed self attestation.");
                }
                return AttestationTrust.Self;
            }
            if (statement.Count != 3)
            {
                throw new PasskeyVerificationException("Invalid packed attestation fields.");
            }
            byte[][] certificates = statement.Certificates();
            using X509Certificate2 leaf = X509CertificateLoader.LoadCertificate(certificates[0]);
            CertificateValidation.CheckLeaf(leaf, context.Credential.AaGuid);
            Dictionary<string, string> subject = CertificateValidation.ReadName(leaf.SubjectName.RawData);
            if (!subject.TryGetValue("2.5.4.6", out string? country) || country.Length != 2
                || !subject.TryGetValue("2.5.4.10", out string? organization) || string.IsNullOrWhiteSpace(organization)
                || !subject.TryGetValue("2.5.4.11", out string? unit) || unit != "Authenticator Attestation"
                || !subject.TryGetValue("2.5.4.3", out string? name) || string.IsNullOrWhiteSpace(name)
                || !CertificateValidation.VerifySignature(leaf, algorithm, context.SignedData, signature))
            {
                throw new PasskeyVerificationException("Invalid packed attestation certificate or signature.");
            }
            return CertificateValidation.Trust(certificates, options);
        }

        private static AttestationTrust VerifyU2f(CborMap statement, AttestationContext context, PasskeyServerOptions options)
        {
            byte[][] certificates = statement.Certificates();
            if (statement.Count != 2 || certificates.Length != 1 || context.Credential.AaGuid != Guid.Empty
                || context.Key.Algorithm != CoseAlgorithm.ES256)
            {
                throw new PasskeyVerificationException("Invalid FIDO U2F attestation.");
            }
            byte[] data = [0, .. context.AuthenticatorData.AsSpan(0, 32), .. context.ClientDataHash,
                .. context.Credential.CredentialId, 4, .. context.Key.X, .. context.Key.Y];
            using X509Certificate2 leaf = X509CertificateLoader.LoadCertificate(certificates[0]);
            if (!CertificateValidation.VerifySignature(leaf, CoseAlgorithm.ES256, data, statement.Bytes("sig")))
            {
                throw new PasskeyVerificationException("Invalid FIDO U2F signature.");
            }
            return CertificateValidation.Trust(certificates, options);
        }

        private static AttestationTrust VerifyApple(CborMap statement, AttestationContext context, PasskeyServerOptions options)
        {
            int expectedFields = 1;
            if (statement.Contains("alg"))
            {
                expectedFields++;
                if (statement.Integer("alg") != (int)context.Key.Algorithm)
                {
                    throw new PasskeyVerificationException("Apple attestation algorithm does not match the credential key.");
                }
            }
            if (statement.Count != expectedFields)
            {
                throw new PasskeyVerificationException("Invalid Apple attestation fields.");
            }
            byte[][] certificates = statement.Certificates();
            using X509Certificate2 leaf = X509CertificateLoader.LoadCertificate(certificates[0]);
            X509Extension extension = leaf.Extensions["1.2.840.113635.100.8.2"]
                ?? throw new PasskeyVerificationException("Missing Apple attestation nonce.");
            AsnReader reader = new(extension.RawData, AsnEncodingRules.DER);
            AsnReader sequence = reader.ReadSequence();
            AsnReader tagged = sequence.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true));
            byte[] nonce = tagged.ReadOctetString();
            tagged.ThrowIfNotEmpty();
            sequence.ThrowIfNotEmpty();
            reader.ThrowIfNotEmpty();
            if (!CryptographicOperations.FixedTimeEquals(nonce, SHA256.HashData(context.SignedData))
                || !CertificateValidation.MatchesKey(leaf, context.Key))
            {
                throw new PasskeyVerificationException("Apple attestation nonce or public key does not match.");
            }
            return CertificateValidation.Trust(certificates, options);
        }
    }
}
