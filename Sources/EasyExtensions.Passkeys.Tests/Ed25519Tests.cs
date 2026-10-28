// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using EasyExtensions.Passkeys.Cryptography;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class Ed25519Tests
    {
        private const string PublicKey = "d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a";
        private const string EmptySignature = "e5564300c360ac729086e2cc806e828a84877f1eb8e5d974d873e06522490155"
            + "5fb8821590a33bacc61e39701cf9b46bd25bf5f0595bbe24655141438e7a100b";

        [TestCase(PublicKey, "", EmptySignature)]
        [TestCase("3d4017c3e843895a92b70aa74d1b7ebc9c982ccf2ec4968cc0cd55f12af4660c", "72",
            "92a009a9f0d4cab8720e820b5f642540a2b27b5416503f8fb3762223ebdb69da"
            + "085ac1e43e15996e458f3613d0f11d8c387b2eaeb4302aeeb00d291612bb0c00")]
        [TestCase("fc51cd8e6218a1a38da47ed00230f0580816ed13ba3303ac5deb911548908025", "af82",
            "6291d657deec24024827e69c3abe01a30ce548a284743a445e3680d7db5ac3ac"
            + "18ff9b538d16f290ae67f760984dc6594a7c15e9716ed28dc027beceea1ec40a")]
        public void Rfc8032SignaturesAreVerified(string publicKey, string message, string signature)
        {
            byte[] key = Convert.FromHexString(publicKey);
            byte[] bytes = Convert.FromHexString(signature);
            Assert.That(Ed25519Signature.Verify(key, Convert.FromHexString(message), bytes), Is.True);
            bytes[0] ^= 1;
            Assert.That(Ed25519Signature.Verify(key, Convert.FromHexString(message), bytes), Is.False);
        }

        [Test]
        public void NoncanonicalScalarsAndSmallOrderKeysAreRejected()
        {
            byte[] signature = Convert.FromHexString(EmptySignature);
            BigInteger order = (BigInteger.One << 252) + BigInteger.Parse("27742317777372353535851937790883648493");
            BigInteger scalar = new(signature.AsSpan(32), isUnsigned: true, isBigEndian: false);
            (scalar + order).TryWriteBytes(signature.AsSpan(32), out _, isUnsigned: true, isBigEndian: false);
            Assert.That(Ed25519Signature.Verify(Convert.FromHexString(PublicKey), [], signature), Is.False);
            byte[] identity = new byte[32];
            identity[0] = 1;
            Assert.That(Ed25519Signature.Verify(identity, [], Convert.FromHexString(EmptySignature)), Is.False);
            Assert.That(Ed25519Signature.Verify(new byte[32], [], new byte[64]), Is.False);
        }

        [Test]
        public void StoredEd25519CoseKeyVerifiesIndependentAssertion()
        {
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions() with { Challenge = new byte[32] };
            byte[] data = [.. SHA256.HashData(Encoding.UTF8.GetBytes("example.com")), 5, 0, 0, 0, 0];
            byte[] signature = Convert.FromHexString("b7de36fbc7605584fe97ab32ea20e70906d9af46ea6661dfa02731ce04d80c352"
                + "6e7ab7ff4ccfd871f6be547d1f9218078392e87b9ba30c199831683f98dcf07");
            byte[] cose = [0xA4, 1, 1, 3, 0x27, 0x20, 6, 0x21, 0x58, 0x20, .. Convert.FromHexString(PublicKey)];
            StoredCredential credential = new([1, 2, 3], cose, BrowserCredential.User.Id, 0, false);
            AuthenticationResponse response = new("AQID", credential.CredentialId, "public-key",
                new AssertionResponse(BrowserCredential.ClientData("webauthn.get", options.Challenge, BrowserCredential.Origin), data, signature)
                { UserHandle = BrowserCredential.User.Id });
            Assert.That(server.VerifyAuthentication(response, options, credential).CredentialId, Is.EqualTo(credential.CredentialId));
        }
    }
}
