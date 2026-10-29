// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Cbor;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class AppleAttestationTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void MatchingAlgorithmPreservesAttestationTrust(bool trusted)
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServerOptions configuration = BrowserCredential.Configuration;
            if (trusted)
            {
                configuration = configuration with
                {
                    RequireTrustedAttestation = true,
                    AttestationTrustRoots = [certificates.Root.RawData]
                };
            }
            PasskeyServer server = new(configuration);
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = certificates.Response(browser, options, "apple");
            RegistrationResult expected = server.VerifyRegistration(response, options);
            RegistrationResult actual = server.VerifyRegistration(AddField(response, "alg", EncodeInteger(-7)), options);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.Trust, Is.EqualTo(expected.Trust));
                Assert.That(actual.CosePublicKey, Is.EqualTo(expected.CosePublicKey));
                Assert.That(actual.CredentialId, Is.EqualTo(expected.CredentialId));
            }
        }

        [TestCase(-257)]
        [TestCase(-8)]
        [TestCase(0)]
        public void MismatchedAlgorithmIsRejected(int algorithm)
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = AddField(certificates.Response(browser, options, "apple"), "alg", EncodeInteger(algorithm));

            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
        }

        [Test]
        public void NonIntegerAlgorithmIsRejected()
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            CborWriter value = new();
            value.WriteTextString("-7");
            RegistrationResponse response = AddField(certificates.Response(browser, options, "apple"), "alg", value.Encode());

            Assert.Throws<InvalidOperationException>(() => server.VerifyRegistration(response, options));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UnknownFieldsAreRejected(bool includeAlgorithm)
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = certificates.Response(browser, options, "apple");
            if (includeAlgorithm)
            {
                response = AddField(response, "alg", EncodeInteger(-7));
            }
            response = AddField(response, "unexpected", EncodeInteger(1));

            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
        }

        [Test]
        public void OptionalAlgorithmDoesNotBypassNonceVerification()
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = certificates.Response(browser, options, "apple", wrongBinding: true);
            response = AddField(response, "alg", EncodeInteger(-7));

            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
        }

        [Test]
        public void OptionalAlgorithmDoesNotBypassRootVerification()
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            using AttestationFixture unrelated = new();
            PasskeyServer server = new(BrowserCredential.Configuration with
            {
                RequireTrustedAttestation = true, AttestationTrustRoots = [unrelated.Root.RawData]
            });
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = AddField(certificates.Response(browser, options, "apple"), "alg", EncodeInteger(-7));

            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
        }

        private static byte[] EncodeInteger(int value)
        {
            CborWriter writer = new();
            writer.WriteInt32(value);
            return writer.Encode();
        }

        private static RegistrationResponse AddField(RegistrationResponse response, string name, byte[] value)
        {
            CborReader reader = new(response.Response.AttestationObject);
            int count = reader.ReadStartMap() ?? throw new InvalidOperationException();
            CborWriter writer = new();
            writer.WriteStartMap(count);
            for (int index = 0; index < count; index++)
            {
                string field = reader.ReadTextString();
                writer.WriteTextString(field);
                if (field == "attStmt")
                {
                    int fields = reader.ReadStartMap() ?? throw new InvalidOperationException();
                    writer.WriteStartMap(fields + 1);
                    for (int item = 0; item < fields; item++)
                    {
                        writer.WriteEncodedValue(reader.ReadEncodedValue().Span);
                        writer.WriteEncodedValue(reader.ReadEncodedValue().Span);
                    }
                    reader.ReadEndMap();
                    writer.WriteTextString(name);
                    writer.WriteEncodedValue(value);
                    writer.WriteEndMap();
                }
                else
                {
                    writer.WriteEncodedValue(reader.ReadEncodedValue().Span);
                }
            }
            reader.ReadEndMap();
            writer.WriteEndMap();
            return response with { Response = response.Response with { AttestationObject = writer.Encode() } };
        }
    }
}
