// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Cbor;
using System.Security.Cryptography;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class RsaCredentialTests
    {
        [TestCase(-3)]
        [TestCase(-4)]
        [TestCase(-5)]
        [TestCase(-6)]
        [TestCase(-7)]
        [TestCase(-8)]
        [TestCase(-9)]
        public void RegistrationRejectsPrivateRsaParameters(int label)
        {
            using BrowserCredential browser = new(CoseAlgorithm.RS256);
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions options = server.CreateRegistrationOptions(BrowserCredential.User);
            byte[] key = AddPrivateParameter(browser, label);
            byte[] data = browser.AuthenticatorData(0x45, 0, registration: true);
            data = [.. data.AsSpan(0, data.Length - browser.PublicKey.Length), .. key];
            RegistrationResponse response = browser.Register(options);
            byte[] signature = browser.Sign([.. data, .. SHA256.HashData(response.Response.ClientDataJSON)]);
            response = response with { Response = response.Response with
            {
                AttestationObject = BrowserCredential.Attestation("packed", data, browser.PackedStatement(signature))
            } };

            Assert.Throws<PasskeyVerificationException>(() => server.VerifyRegistration(response, options));
        }

        [TestCase(-3)]
        [TestCase(-4)]
        [TestCase(-5)]
        [TestCase(-6)]
        [TestCase(-7)]
        [TestCase(-8)]
        [TestCase(-9)]
        public void AuthenticationRejectsStoredPrivateRsaParameters(int label)
        {
            using BrowserCredential browser = new(CoseAlgorithm.RS256);
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            StoredCredential credential = browser.Stored() with { CosePublicKey = AddPrivateParameter(browser, label) };

            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options), options, credential));
        }

        private static byte[] AddPrivateParameter(BrowserCredential browser, int label)
        {
            RSAParameters parameters = browser.RsaParameters(includePrivateParameters: true);
            CborReader reader = new(browser.PublicKey);
            int count = reader.ReadStartMap() ?? throw new InvalidOperationException();
            CborWriter writer = new(CborConformanceMode.Ctap2Canonical);
            writer.WriteStartMap(count + 1);
            for (int index = 0; index < count; index++)
            {
                writer.WriteEncodedValue(reader.ReadEncodedValue().Span);
                writer.WriteEncodedValue(reader.ReadEncodedValue().Span);
            }
            reader.ReadEndMap();
            writer.WriteInt32(label);
            if (label == -9)
            {
                writer.WriteStartArray(1);
                writer.WriteStartMap(3);
                writer.WriteInt32(-10);
                writer.WriteByteString(parameters.P!);
                writer.WriteInt32(-11);
                writer.WriteByteString(parameters.DP!);
                writer.WriteInt32(-12);
                writer.WriteByteString(parameters.InverseQ!);
                writer.WriteEndMap();
                writer.WriteEndArray();
            }
            else
            {
                byte[] value = label switch
                {
                    -3 => parameters.D!,
                    -4 => parameters.P!,
                    -5 => parameters.Q!,
                    -6 => parameters.DP!,
                    -7 => parameters.DQ!,
                    -8 => parameters.InverseQ!,
                    _ => throw new ArgumentOutOfRangeException(nameof(label))
                };
                writer.WriteByteString(value);
            }
            writer.WriteEndMap();
            return writer.Encode();
        }
    }
}
