// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Formats.Cbor;
using System.Security.Cryptography;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class CoseRejectionTests
    {
        [TestCase((int)CoseAlgorithm.RS256, 1, false, 32)]
        [TestCase((int)CoseAlgorithm.ES256, 2, false, 32)]
        [TestCase((int)CoseAlgorithm.ES256, 1, true, 32)]
        [TestCase((int)CoseAlgorithm.ES256, 1, false, 31)]
        [TestCase(-65535, 1, false, 32)]
        public void AlgorithmConfusionPrivateMaterialAndWrongCoordinatesAreRejected(int algorithm, int curve, bool privateKey, int coordinateLength)
        {
            using BrowserCredential browser = new();
            ECParameters parameters = browser.EcParameters();
            CborWriter writer = new();
            writer.WriteStartMap(privateKey ? 6 : 5);
            writer.WriteInt32(1);
            writer.WriteInt32(2);
            writer.WriteInt32(3);
            writer.WriteInt32(algorithm);
            writer.WriteInt32(-1);
            writer.WriteInt32(curve);
            writer.WriteInt32(-2);
            writer.WriteByteString(parameters.Q.X!.AsSpan(0, coordinateLength));
            writer.WriteInt32(-3);
            writer.WriteByteString(parameters.Q.Y!);
            if (privateKey)
            {
                writer.WriteInt32(-4);
                writer.WriteByteString([1]);
            }
            writer.WriteEndMap();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            StoredCredential credential = browser.Stored() with { CosePublicKey = writer.Encode() };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(browser.Authenticate(options), options, credential));
        }

        [Test]
        public void DuplicateCoseMapKeysAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            StoredCredential credential = browser.Stored() with { CosePublicKey = [0xA2, 1, 2, 1, 2] };
            Assert.Throws<CborContentException>(() => server.VerifyAuthentication(browser.Authenticate(options), options, credential));
        }
    }
}
