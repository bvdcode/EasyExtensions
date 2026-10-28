// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class CeremonyTests
    {
        [TestCase(CoseAlgorithm.ES256)]
        [TestCase(CoseAlgorithm.ES384)]
        [TestCase(CoseAlgorithm.ES512)]
        [TestCase(CoseAlgorithm.ES256K)]
        [TestCase(CoseAlgorithm.RS256)]
        [TestCase(CoseAlgorithm.RS384)]
        [TestCase(CoseAlgorithm.RS512)]
        [TestCase(CoseAlgorithm.PS256)]
        [TestCase(CoseAlgorithm.PS384)]
        [TestCase(CoseAlgorithm.PS512)]
        public void RegistrationAndAuthenticationPreserveCoseKey(CoseAlgorithm algorithm)
        {
            using BrowserCredential browser = new(algorithm);
            PasskeyServer server = new(BrowserCredential.Configuration with { Algorithms = [algorithm] });
            RegistrationOptions registration = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResult registered = server.VerifyRegistration(browser.Register(registration, counter: 7), registration);
            Assert.That(registered.CosePublicKey, Is.EqualTo(browser.PublicKey));
            Assert.That(registered.AaGuid, Is.EqualTo(browser.AaGuid));
            Assert.That(registered.Trust, Is.EqualTo(AttestationTrust.Self));
            StoredCredential stored = new(registered.CredentialId, registered.CosePublicKey, registered.UserHandle,
                registered.SignatureCounter, registered.IsBackupEligible);
            AuthenticationOptions authentication = server.CreateAuthenticationOptions();
            AuthenticationResult result = server.VerifyAuthentication(browser.Authenticate(authentication, counter: 8), authentication, stored);
            Assert.That(result.SignatureCounter, Is.EqualTo(8));
        }

        [Test]
        public void ZeroCountersAndBackedUpCredentialsAreSupported()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions registration = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationResult result = server.VerifyRegistration(browser.Register(registration, "none", flags: 0x5D), registration);
            Assert.That(result.IsBackupEligible, Is.True);
            Assert.That(result.IsBackedUp, Is.True);
            Assert.That(result.Trust, Is.EqualTo(AttestationTrust.None));
            AuthenticationOptions authentication = server.CreateAuthenticationOptions();
            AuthenticationResult assertion = server.VerifyAuthentication(browser.Authenticate(authentication, flags: 0x0D), authentication,
                browser.Stored(backupEligible: true));
            Assert.That(assertion.SignatureCounter, Is.Zero);
            Assert.That(assertion.IsBackedUp, Is.False);
        }

        [Test]
        public void UsernameLoginCanOmitUserHandleWhenCredentialIsAllowed()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions([new CredentialDescriptor(browser.Id)]);
            AuthenticationResponse response = browser.Authenticate(options);
            response = response with { Response = response.Response with { UserHandle = null } };
            Assert.That(server.VerifyAuthentication(response, options, browser.Stored()).CredentialId, Is.EqualTo(browser.Id));
        }

        [Test]
        public void RequiredVerificationAndResidentKeysAreRequestedByDefault()
        {
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions first = server.CreateRegistrationOptions(BrowserCredential.User);
            RegistrationOptions second = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.That(first.AuthenticatorSelection.UserVerification, Is.EqualTo(UserVerificationRequirement.Required));
            Assert.That(first.AuthenticatorSelection.ResidentKey, Is.EqualTo(ResidentKeyRequirement.Required));
            Assert.That(first.Challenge, Has.Length.EqualTo(32));
            Assert.That(first.Challenge, Is.Not.EqualTo(second.Challenge));
        }

        [Test]
        public void ConfigurationCollectionsAreCopied()
        {
            string[] origins = [BrowserCredential.Origin];
            CoseAlgorithm[] algorithms = [CoseAlgorithm.ES256];
            PasskeyServer server = new(BrowserCredential.Configuration with { AllowedOrigins = origins, Algorithms = algorithms });
            origins[0] = "https://attacker.example.com";
            algorithms[0] = CoseAlgorithm.RS256;
            using BrowserCredential browser = new();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            Assert.That(server.VerifyRegistration(browser.Register(options), options).Trust, Is.EqualTo(AttestationTrust.Self));
        }
    }
}
