// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text;
using System.Text.Json;
using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys.Tests
{
    public class JsonTests
    {
        [Test]
        public void OptionsUseBrowserJsonContractsAndRoundTrip()
        {
            PasskeyServer server = BrowserCredential.Server();
            RegistrationOptions original = server.CreateRegistrationOptions(BrowserCredential.User);
            string json = PasskeyJson.Serialize(original);
            using JsonDocument document = JsonDocument.Parse(json);
            Assert.That(document.RootElement.GetProperty("authenticatorSelection").GetProperty("userVerification").GetString(), Is.EqualTo("required"));
            Assert.That(document.RootElement.GetProperty("authenticatorSelection").GetProperty("residentKey").GetString(), Is.EqualTo("required"));
            Assert.That(document.RootElement.GetProperty("pubKeyCredParams")[0].GetProperty("alg").GetInt32(), Is.EqualTo(-7));
            Assert.That(PasskeyJson.ReadRegistrationOptions(json).Challenge, Is.EqualTo(original.Challenge));
            AuthenticationOptions authentication = server.CreateAuthenticationOptions();
            Assert.That(PasskeyJson.ReadAuthenticationOptions(PasskeyJson.Serialize(authentication)).Challenge, Is.EqualTo(authentication.Challenge));
        }

        [TestCase("AA==")]
        [TestCase("A+")]
        [TestCase("A/")]
        [TestCase("A")]
        [TestCase("AB")]
        [TestCase("AAF")]
        [TestCase("AA ")]
        [TestCase("AA\\n")]
        [TestCase("AĀ")]
        public void InvalidOrNoncanonicalBase64UrlIsRejected(string rawId)
        {
            string json = "{\"id\":\"AA\",\"type\":\"public-key\",\"rawId\":\"" + rawId
                + "\",\"response\":{\"clientDataJSON\":\"AA\",\"authenticatorData\":\"AA\",\"signature\":\"AA\"}}";
            Assert.Throws<JsonException>(() => PasskeyJson.ReadAuthentication(json));
        }

        [TestCase("{}")]
        [TestCase("null")]
        [TestCase("{\"id\":null,\"rawId\":\"AA\",\"type\":\"public-key\",\"response\":null}")]
        [TestCase("{\"id\":\"AA\",\"id\":\"AQ\"}")]
        public void MissingNullAndDuplicateFieldsAreRejected(string json)
        {
            Assert.Throws<JsonException>(() => PasskeyJson.ReadAuthentication(json));
        }

        [TestCase("{\"value\":1,\"value\":2}")]
        [TestCase("[{\"value\":1,\"value\":2}]")]
        [TestCase("{\"value\":1,\"\\u0076alue\":2}")]
        public void DuplicatePropertiesInUnmappedObjectsAreRejected(string extension)
        {
            string json = "{\"id\":\"AA\",\"type\":\"public-key\",\"rawId\":\"AA\","
                + "\"response\":{\"clientDataJSON\":\"AA\",\"authenticatorData\":\"AA\",\"signature\":\"AA\"},"
                + "\"extension\":" + extension + "}";
            Assert.Throws<JsonException>(() => PasskeyJson.ReadAuthentication(json));
        }

        [Test]
        public void UnmappedObjectsWithoutDuplicatesRemainAllowed()
        {
            string json = "{\"id\":\"AA\",\"type\":\"public-key\",\"rawId\":\"AA\","
                + "\"response\":{\"clientDataJSON\":\"AA\",\"authenticatorData\":\"AA\",\"signature\":\"AA\"},"
                + "\"extension\":{\"value\":1}}";
            Assert.That(PasskeyJson.ReadAuthentication(json).RawId, Is.EqualTo(new byte[] { 0 }));
        }

        [Test]
        public void DuplicateClientDataPropertiesAreRejected()
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            AuthenticationResponse response = browser.Authenticate(options);
            string original = Encoding.UTF8.GetString(response.Response.ClientDataJSON);
            byte[] duplicate = Encoding.UTF8.GetBytes(original.Replace("{", "{\"type\":\"webauthn.get\",", StringComparison.Ordinal));
            response = response with { Response = response.Response with { ClientDataJSON = duplicate } };
            Assert.Throws<JsonException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
        }

        [TestCase("{}")]
        [TestCase("[]")]
        [TestCase("{\"type\":false}")]
        public void MissingOrIncorrectlyTypedClientDataIsRejected(string json)
        {
            using BrowserCredential browser = new();
            PasskeyServer server = BrowserCredential.Server();
            AuthenticationOptions options = server.CreateAuthenticationOptions();
            AuthenticationResponse response = browser.Authenticate(options);
            response = response with { Response = response.Response with { ClientDataJSON = Encoding.UTF8.GetBytes(json) } };
            Assert.Throws<PasskeyVerificationException>(() => server.VerifyAuthentication(response, options, browser.Stored()));
        }

        [TestCase("https://login.example.com/")]
        [TestCase("https://login.example.com/path")]
        [TestCase("http://login.example.com")]
        [TestCase("https://example.com.attacker.test")]
        [TestCase("https://user@login.example.com")]
        [TestCase("https://login.example.com?query=1")]
        public void InvalidOriginConfigurationIsRejected(string origin)
        {
            Assert.Throws<ArgumentException>(() => new PasskeyServer(BrowserCredential.Configuration with { AllowedOrigins = [origin] }));
        }

        [Test]
        public void LocalhostHttpIsSupportedExplicitly()
        {
            Assert.DoesNotThrow(() => new PasskeyServer(new PasskeyServerOptions("localhost", "Example", ["http://localhost:5000"])));
        }
    }
}
