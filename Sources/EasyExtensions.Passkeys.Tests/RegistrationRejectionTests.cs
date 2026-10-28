// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Cbor;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class RegistrationRejectionTests
    {
        [TestCase("android-safetynet")]
        [TestCase("unknown")]
        [TestCase("Packed")]
        public void UnsupportedFormatsAreRejected(string format)
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(options, format), options));
        }

        [Test]
        public void NoneAttestationCannotContainAnUnverifiedStatement()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(options, "none", statement: browser.PackedStatement([1])), options));
        }

        [Test]
        public void SelfAttestationSignatureAndAlgorithmAreVerified()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(options, statement: browser.PackedStatement([1, 2, 3])), options));
            CborWriter writer = new();
            writer.WriteStartMap(2);
            writer.WriteTextString("alg");
            writer.WriteInt32((int)CoseAlgorithm.RS256);
            writer.WriteTextString("sig");
            writer.WriteByteString([1]);
            writer.WriteEndMap();
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(options, statement: writer.Encode()), options));
        }

        [Test]
        public void ExcludedCredentialsAndWrongIdsAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions excluded = server.CreateRegistrationOptions(BrowserCredential.User, [new CredentialDescriptor(browser.Id)]);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(excluded), excluded));
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = browser.Register(options) with { RawId = [1], Id = "AQ" };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
            response = browser.Register(options) with { Id = "incorrect" };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
        }

        [Test]
        public void UnofferedAlgorithmsAreRejected()
        {
            using BrowserCredential browser = new(CoseAlgorithm.RS256);
            PasskeyServer server = new(BrowserCredential.Configuration with { Algorithms = [CoseAlgorithm.ES256] });
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(options), options));
        }

        [Test]
        public void TrustedAttestationPolicyRejectsNoneAndSelfAttestation()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = new(BrowserCredential.Configuration with { RequireTrustedAttestation = true });
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(options, "none"), options));
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(browser.Register(options), options));
        }

        [Test]
        public void InvalidCborAndTrailingDataAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = browser.Register(options);
            byte[] malformed = [.. response.Response.AttestationObject, 0];
            response = response with { Response = response.Response with { AttestationObject = malformed } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
            response = response with { Response = response.Response with { AttestationObject = [0xBF, 0xFF] } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
        }
    }
}
