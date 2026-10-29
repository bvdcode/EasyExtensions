// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Security.Cryptography;
using EasyExtensions.Passkeys.Attestation;
using EasyExtensions.Passkeys.Cryptography;
using EasyExtensions.Passkeys.Models;
using EasyExtensions.Passkeys.Protocol;
using EasyExtensions.Passkeys.Serialization;

namespace EasyExtensions.Passkeys
{
    public class PasskeyServer : IPasskeyServer
    {
        private readonly ServerConfiguration configuration;
        private PasskeyServerOptions Options => configuration.Options;

        public PasskeyServer(PasskeyServerOptions options)
        {
            configuration = new ServerConfiguration(options);
        }

        public RegistrationOptions CreateRegistrationOptions(PasskeyUser user, IReadOnlyList<CredentialDescriptor>? excludeCredentials = null)
        {
            ArgumentNullException.ThrowIfNull(user);
            if (user.Id.Length is 0 or > 64 || string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.DisplayName))
            {
                throw new ArgumentException("A user handle of 1–64 bytes, name and display name are required.", nameof(user));
            }
            return new RegistrationOptions(new RelyingParty(Options.RelyingPartyId, Options.RelyingPartyName),
                user with { Id = user.Id.ToArray() }, RandomNumberGenerator.GetBytes(ProtocolLimits.ChallengeLength),
                Options.Algorithms.Select(algorithm => new CredentialParameter(algorithm)).ToArray(),
                CopyDescriptors(excludeCredentials), new AuthenticatorSelection(Options.ResidentKey, Options.UserVerification),
                Options.Attestation, Options.TimeoutMilliseconds);
        }

        public AuthenticationOptions CreateAuthenticationOptions(IReadOnlyList<CredentialDescriptor>? allowCredentials = null)
        {
            return new AuthenticationOptions(Options.RelyingPartyId, RandomNumberGenerator.GetBytes(ProtocolLimits.ChallengeLength),
                CopyDescriptors(allowCredentials), Options.UserVerification, Options.TimeoutMilliseconds);
        }

        public RegistrationResult VerifyRegistration(RegistrationResponse response, RegistrationOptions originalOptions)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(originalOptions);
            ValidateCredentialId(response.Id, response.RawId, response.Type);
            if (originalOptions.Rp.Id != Options.RelyingPartyId || originalOptions.User.Id.Length is 0 or > 64
                || originalOptions.ExcludeCredentials.Any(credential => credential.Id.AsSpan().SequenceEqual(response.RawId)))
            {
                throw new PasskeyVerificationException("The registration options or excluded credential do not match.");
            }
            byte[] clientHash = ClientData.Verify(response.Response.ClientDataJSON, "webauthn.create", originalOptions.Challenge, configuration.Origins);
            if (response.Response.AttestationObject.Length is 0 or > ProtocolLimits.MaximumAttestationLength)
            {
                throw new PasskeyVerificationException("Invalid attestation object size.");
            }
            CborMap attestation = CborMap.Read(response.Response.AttestationObject);
            if (attestation.Count != 3)
            {
                throw new PasskeyVerificationException("Invalid attestation object fields.");
            }
            byte[] rawData = attestation.Bytes("authData");
            bool requireVerification = ServerConfiguration.RequiresVerification(Options.UserVerification)
                || ServerConfiguration.RequiresVerification(originalOptions.AuthenticatorSelection.UserVerification);
            AuthenticatorData credential = new(rawData, Options.RelyingPartyId, registration: true, requireVerification);
            CoseKey key = credential.Key ?? throw new PasskeyVerificationException("Missing credential public key.");
            if (!credential.CredentialId.AsSpan().SequenceEqual(response.RawId)
                || !Options.Algorithms.Contains(key.Algorithm)
                || !originalOptions.PubKeyCredParams.Any(parameter => parameter.Alg == key.Algorithm))
            {
                throw new PasskeyVerificationException("Credential ID or public key algorithm does not match.");
            }
            string format = attestation.Text("fmt");
            AttestationTrust trust = AttestationVerifier.Verify(format, CborMap.Read(attestation.Get("attStmt")),
                new AttestationContext(rawData, clientHash, credential), Options);
            return new RegistrationResult(credential.CredentialId, credential.PublicKey, originalOptions.User.Id.ToArray(),
                credential.SignatureCounter, credential.AaGuid, credential.IsBackupEligible, credential.IsBackedUp,
                format, trust, response.Response.Transports.ToArray());
        }

        public AuthenticationResult VerifyAuthentication(AuthenticationResponse response, AuthenticationOptions originalOptions, StoredCredential credential)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(originalOptions);
            ArgumentNullException.ThrowIfNull(credential);
            ValidateCredentialId(response.Id, response.RawId, response.Type);
            ValidateAuthenticationCredential(response, originalOptions, credential);
            byte[] clientHash = ClientData.Verify(response.Response.ClientDataJSON, "webauthn.get", originalOptions.Challenge, configuration.Origins);
            bool requireVerification = ServerConfiguration.RequiresVerification(Options.UserVerification)
                || ServerConfiguration.RequiresVerification(originalOptions.UserVerification);
            AuthenticatorData data = new(response.Response.AuthenticatorData, Options.RelyingPartyId, registration: false, requireVerification);
            CoseKey key = new(credential.CosePublicKey);
            ValidateCredentialState(data, credential);
            byte[] signedData = [.. response.Response.AuthenticatorData, .. clientHash];
            if (response.Response.Signature.Length is 0 or > 1024 || !key.Verify(signedData, response.Response.Signature))
            {
                throw new PasskeyVerificationException("Invalid assertion signature.");
            }
            return new AuthenticationResult(response.RawId.ToArray(), data.SignatureCounter, data.IsBackedUp);
        }

        private void ValidateAuthenticationCredential(AuthenticationResponse response, AuthenticationOptions originalOptions, StoredCredential credential)
        {
            byte[]? userHandle = response.Response.UserHandle;
            if (originalOptions.RpId != Options.RelyingPartyId || !response.RawId.AsSpan().SequenceEqual(credential.CredentialId)
                || (originalOptions.AllowCredentials.Count != 0 && !originalOptions.AllowCredentials.Any(item => item.Id.AsSpan().SequenceEqual(response.RawId)))
                || (originalOptions.AllowCredentials.Count == 0 && userHandle is null)
                || credential.UserHandle.Length is 0 or > 64
                || (userHandle is not null && !CryptographicOperations.FixedTimeEquals(userHandle, credential.UserHandle)))
            {
                throw new PasskeyVerificationException("Credential or user handle does not match the authentication options.");
            }
        }

        private static void ValidateCredentialState(AuthenticatorData data, StoredCredential credential)
        {
            if (data.IsBackupEligible != credential.IsBackupEligible
                || ((data.SignatureCounter != 0 || credential.SignatureCounter != 0) && data.SignatureCounter <= credential.SignatureCounter))
            {
                throw new PasskeyVerificationException("Backup eligibility changed or the signature counter did not advance.");
            }
        }

        private static void ValidateCredentialId(string id, byte[] rawId, string type)
        {
            ArgumentNullException.ThrowIfNull(rawId);
            if (type != "public-key" || rawId.Length is 0 or > ProtocolLimits.MaximumCredentialIdLength || id != Base64UrlConverter.Encode(rawId))
            {
                throw new PasskeyVerificationException("Invalid credential type or ID.");
            }
        }

        private static CredentialDescriptor[] CopyDescriptors(IReadOnlyList<CredentialDescriptor>? descriptors)
        {
            if (descriptors is null)
            {
                return [];
            }
            return descriptors.Select(descriptor => descriptor with { Id = descriptor.Id.ToArray(), Transports = descriptor.Transports.ToArray() }).ToArray();
        }
    }
}
