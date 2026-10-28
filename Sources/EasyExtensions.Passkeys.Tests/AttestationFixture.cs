// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Asn1;
using System.Formats.Cbor;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using EasyExtensions.Passkeys.Models;
using EasyExtensions.Passkeys.Serialization;

namespace EasyExtensions.Passkeys.Tests
{
    internal class AttestationFixture : IDisposable
    {
        private readonly ECDsa rootKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        internal X509Certificate2 Root { get; }

        internal AttestationFixture()
        {
            CertificateRequest request = new("CN=Test Root", rootKey, HashAlgorithmName.SHA256);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign, true));
            Root = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(2));
        }

        internal RegistrationResponse Response(BrowserCredential browser, RegistrationOptions options, string format,
            bool wrongBinding = false, bool expired = false, bool allApplications = false, bool caLeaf = false)
        {
            if (format == "fido-u2f")
            {
                browser.AaGuid = Guid.Empty;
            }
            byte[] client = BrowserCredential.ClientData("webauthn.create", options.Challenge, BrowserCredential.Origin);
            byte[] hash = SHA256.HashData(client);
            byte[] data = browser.AuthenticatorData(0x45, 0, registration: true);
            byte[] signed = [.. data, .. hash];
            string subject = format switch
            {
                "tpm" => string.Empty,
                "packed" => "C=US, O=Example, OU=Authenticator Attestation, CN=Test Authenticator",
                _ => "CN=Test Authenticator"
            };
            CertificateRequest request = browser.CertificateRequest(subject);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(caLeaf, false, 0, true));
            switch (format)
            {
                case "apple":
                    byte[] nonce = SHA256.HashData(signed);
                    if (wrongBinding)
                    {
                        nonce[0] ^= 1;
                    }
                    AsnWriter apple = new(AsnEncodingRules.DER);
                    apple.PushSequence();
                    Asn1Tag nonceTag = new(TagClass.ContextSpecific, 1, isConstructed: true);
                    apple.PushSequence(nonceTag);
                    apple.WriteOctetString(nonce);
                    apple.PopSequence(nonceTag);
                    apple.PopSequence();
                    request.CertificateExtensions.Add(new X509Extension("1.2.840.113635.100.8.2", apple.Encode(), false));
                    break;
                case "android-key":
                    byte[] challenge = hash.ToArray();
                    if (wrongBinding)
                    {
                        challenge[0] ^= 1;
                    }
                    request.CertificateExtensions.Add(new X509Extension("1.3.6.1.4.1.11129.2.1.17", AndroidExtension(challenge, allApplications), false));
                    break;
                case "tpm":
                    request.CertificateExtensions.Add(new X509Extension("2.5.29.17", TpmFixture.Subject(), true));
                    request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension([new Oid("2.23.133.8.3")], false));
                    break;
            }
            DateTimeOffset until = DateTimeOffset.UtcNow.AddDays(1);
            if (expired)
            {
                until = DateTimeOffset.UtcNow.AddDays(-1);
            }
            using X509Certificate2 certificate = request.Create(Root, DateTimeOffset.UtcNow.AddDays(-2), until, RandomNumberGenerator.GetBytes(16));
            byte[] statement;
            switch (format)
            {
                case "packed":
                case "android-key":
                    byte[] signature = browser.Sign(signed);
                    if (wrongBinding && format == "packed")
                    {
                        signature[^1] ^= 1;
                    }
                    statement = browser.PackedStatement(signature, certificate.RawData);
                    break;
                case "apple":
                    CborWriter appleStatement = new();
                    appleStatement.WriteStartMap(1);
                    Certificates(appleStatement, certificate.RawData);
                    appleStatement.WriteEndMap();
                    statement = appleStatement.Encode();
                    break;
                case "fido-u2f":
                    ECParameters key = browser.EcParameters();
                    byte[] u2fData = [0, .. data.AsSpan(0, 32), .. hash, .. browser.Id, 4, .. key.Q.X!, .. key.Q.Y!];
                    byte[] u2fSignature = browser.Sign(u2fData);
                    if (wrongBinding)
                    {
                        u2fSignature[^1] ^= 1;
                    }
                    CborWriter u2f = new();
                    u2f.WriteStartMap(2);
                    Certificates(u2f, certificate.RawData);
                    u2f.WriteTextString("sig");
                    u2f.WriteByteString(u2fSignature);
                    u2f.WriteEndMap();
                    statement = u2f.Encode();
                    break;
                case "tpm":
                    statement = TpmFixture.Statement(browser, signed, certificate.RawData, wrongBinding);
                    break;
                default:
                    throw new ArgumentException("Unsupported test attestation format.", nameof(format));
            }
            return new RegistrationResponse(Base64UrlConverter.Encode(browser.Id), browser.Id, "public-key",
                new AttestationResponse(client, BrowserCredential.Attestation(format, data, statement)));
        }

        private static byte[] AndroidExtension(byte[] challenge, bool allApplications)
        {
            AsnWriter writer = new(AsnEncodingRules.DER);
            writer.PushSequence();
            writer.WriteInteger(3);
            writer.WriteEncodedValue([0x0A, 0x01, 0x01]);
            writer.WriteInteger(4);
            writer.WriteEncodedValue([0x0A, 0x01, 0x01]);
            writer.WriteOctetString(challenge);
            writer.WriteOctetString([]);
            writer.PushSequence();
            writer.PopSequence();
            writer.PushSequence();
            Asn1Tag origin = new(TagClass.ContextSpecific, 702, isConstructed: true);
            writer.PushSequence(origin);
            writer.WriteInteger(0);
            writer.PopSequence(origin);
            Asn1Tag purpose = new(TagClass.ContextSpecific, 1, isConstructed: true);
            writer.PushSequence(purpose);
            writer.PushSetOf();
            writer.WriteInteger(2);
            writer.PopSetOf();
            writer.PopSequence(purpose);
            if (allApplications)
            {
                Asn1Tag all = new(TagClass.ContextSpecific, 600, isConstructed: true);
                writer.PushSequence(all);
                writer.WriteNull();
                writer.PopSequence(all);
            }
            writer.PopSequence();
            writer.PopSequence();
            return writer.Encode();
        }

        internal static void Certificates(CborWriter writer, byte[] certificate)
        {
            writer.WriteTextString("x5c");
            writer.WriteStartArray(1);
            writer.WriteByteString(certificate);
            writer.WriteEndArray();
        }

        public void Dispose()
        {
            Root.Dispose();
            rootKey.Dispose();
        }
    }
}
