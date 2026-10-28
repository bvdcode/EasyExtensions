// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class AttestationTests
    {
        [TestCase("packed")]
        [TestCase("apple")]
        [TestCase("android-key")]
        [TestCase("tpm")]
        [TestCase("fido-u2f")]
        public void CertificateAttestationRequiresExplicitRootForTrust(string format)
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResponse response = certificates.Response(browser, options, format);
            Assert.That(server.VerifyRegistration(response, options).Trust, Is.EqualTo(AttestationTrust.Untrusted));
            PasskeyServer trusted = new(BrowserCredential.Configuration with
            {
                RequireTrustedAttestation = true, AttestationTrustRoots = [certificates.Root.RawData]
            });
            Assert.That(trusted.VerifyRegistration(response, options).Trust, Is.EqualTo(AttestationTrust.Trusted));
        }

        [TestCase("packed")]
        [TestCase("apple")]
        [TestCase("android-key")]
        [TestCase("tpm")]
        [TestCase("fido-u2f")]
        public void InvalidAttestationBindingIsRejected(string format)
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(certificates.Response(browser, options, format, wrongBinding: true), options));
        }

        [TestCase("packed")]
        [TestCase("apple")]
        [TestCase("android-key")]
        [TestCase("tpm")]
        [TestCase("fido-u2f")]
        public void ExpiredAttestationCertificatesAreRejected(string format)
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(certificates.Response(browser, options, format, expired: true), options));
        }

        [Test]
        public void AndroidKeysAvailableToAllApplicationsAreRejected()
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(certificates.Response(browser, options, "android-key", allApplications: true), options));
        }

        [Test]
        public void AnUnrelatedRootDoesNotEstablishTrust()
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            using AttestationFixture unrelated = new();
            PasskeyServer server = new(BrowserCredential.Configuration with
            {
                RequireTrustedAttestation = true, AttestationTrustRoots = [unrelated.Root.RawData]
            });
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(certificates.Response(browser, options, "packed"), options));
        }

        [TestCase("packed")]
        [TestCase("tpm")]
        public void AttestationLeafMustNotBeACertificateAuthority(string format)
        {
            using BrowserCredential browser = new();
            using AttestationFixture certificates = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(certificates.Response(browser, options, format, caLeaf: true), options));
        }
    }
}
