// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class AssertionRejectionTests
    {
        [TestCase("https://evil.example")]
        [TestCase("https://example.com.attacker.test")]
        [TestCase("https://login.example.com:8443")]
        [TestCase("https://login.example.com/")]
        [TestCase("null")]
        public void UnconfiguredOriginsAreRejected(string origin)
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options, origin: origin), options, browser.Stored()));
        }

        [TestCase((byte)0x00)]
        [TestCase((byte)0x01)]
        [TestCase((byte)0x04)]
        [TestCase((byte)0x15)]
        [TestCase((byte)0x45)]
        [TestCase((byte)0x25)]
        [TestCase((byte)0x85)]
        public void InvalidOrMissingFlagsAreRejected(byte flags)
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options, flags: flags), options, browser.Stored()));
        }

        [TestCase((uint)1, (uint)0)]
        [TestCase((uint)5, (uint)5)]
        [TestCase((uint)5, (uint)4)]
        public void NonAdvancingCountersAreRejected(uint previous, uint current)
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options, counter: current), options, browser.Stored(previous)));
        }

        [Test]
        public void AlteredSignatureIsRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            AuthenticationResponse response = browser.Authenticate(options);
            response.Response.Signature[^1] ^= 1;
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
        }

        [Test]
        public void DifferentChallengeAndWrongOperationAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            AuthenticationResponse response = browser.Authenticate(server.CreateAuthenticationOptions());
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
            response = response with { Response = response.Response with
            {
                ClientDataJSON = BrowserCredential.ClientData("webauthn.create", options.Challenge, BrowserCredential.Origin)
            } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
        }

        [Test]
        public void WrongRpIdAndTrailingSignedDataAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            AuthenticationResponse response = browser.Authenticate(options);
            response.Response.AuthenticatorData[0] ^= 1;
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
            response = browser.Authenticate(options);
            response = response with { Response = response.Response with { AuthenticatorData = [.. response.Response.AuthenticatorData, 0] } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
        }

        [Test]
        public void CrossOriginResponsesAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            AuthenticationResponse response = browser.Authenticate(options);
            response = response with { Response = response.Response with
            {
                ClientDataJSON = BrowserCredential.ClientData("webauthn.get", options.Challenge, BrowserCredential.Origin, crossOrigin: true)
            } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
        }

        [Test]
        public void WrongAndMissingUserHandlesAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            AuthenticationResponse response = browser.Authenticate(options);
            response = response with { Response = response.Response with { UserHandle = [9] } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
            response = response with { Response = response.Response with { UserHandle = null } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
        }

        [Test]
        public void CredentialOwnershipAllowListAndBackupEligibilityAreEnforced()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions([new CredentialDescriptor([9])]);
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options), options, browser.Stored()));
            options = server.CreateAuthenticationOptions();
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options, flags: 0x0D), options, browser.Stored()));
            StoredCredential wrongOwner = browser.Stored() with { CredentialId = [9] };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options), options, wrongOwner));
        }
    }
}
