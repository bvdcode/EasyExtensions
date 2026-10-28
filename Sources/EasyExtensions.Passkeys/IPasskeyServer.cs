// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Passkeys.Models;

namespace EasyExtensions.Passkeys
{
    public interface IPasskeyServer
    {
        RegistrationOptions CreateRegistrationOptions(PasskeyUser user, IReadOnlyList<CredentialDescriptor>? excludeCredentials = null);
        RegistrationResult VerifyRegistration(RegistrationResponse response, RegistrationOptions originalOptions);
        AuthenticationOptions CreateAuthenticationOptions(IReadOnlyList<CredentialDescriptor>? allowCredentials = null);
        AuthenticationResult VerifyAuthentication(AuthenticationResponse response, AuthenticationOptions originalOptions, StoredCredential credential);
    }
}
