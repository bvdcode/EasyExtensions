# EasyExtensions.Passkeys

WebAuthn registration and authentication for .NET 10. The package is independent of ASP.NET Core Identity, Entity Framework and application account models.

```sh
dotnet add package EasyExtensions.Passkeys
```

The only runtime NuGet dependency is Microsoft's `System.Formats.Cbor`. There are no Fido2, NSec, libsodium or bundled native binaries. ECDSA, RSA, hashing and certificate chains use .NET cryptography APIs and the platform's cryptographic providers. Ed25519 verification uses managed public-data arithmetic; the package does not implement private-key generation or signing.

## Configure

```csharp
using EasyExtensions.Passkeys;
using EasyExtensions.Passkeys.Models;

IPasskeyServer passkeys = new PasskeyServer(new PasskeyServerOptions(
    "example.com",
    "Example",
    ["https://example.com", "https://login.example.com"]));
```

RP ID is a lowercase DNS hostname, without scheme, port or path. Origins are exact HTTPS origins without trailing slashes; HTTP is accepted only for explicitly configured `localhost` origins. Do not derive this configuration from an untrusted request host.

Defaults request discoverable credentials, require user verification, request direct attestation, and offer ES256, RS256 and Ed25519. Algorithms offered for new registrations are configurable. Authentication supports existing COSE keys using ES256, ES384, ES512, ES256K, RS256/384/512, PS256/384/512 and Ed25519. ES256K availability depends on the platform's curve support. SHA-1 signatures and Ed448 are not supported.

## Registration

```csharp
RegistrationOptions options = passkeys.CreateRegistrationOptions(
    new PasskeyUser(userHandle, userName, displayName), existingCredentialDescriptors);
string optionsJson = PasskeyJson.Serialize(options);
```

Generate `userHandle` once per account: it must be an opaque identifier of 1–64 bytes. Persist the original options on the server with a short expiry and bind them to the authenticated account, session and registration operation. `PasskeyJson.ReadRegistrationOptions` restores options previously serialized by the server.

In a browser that supports the WebAuthn JSON methods:

```javascript
const publicKey = PublicKeyCredential.parseCreationOptionsFromJSON(options);
const credential = await navigator.credentials.create({ publicKey });
const response = credential.toJSON();
```

After retrieving and atomically consuming the pending server challenge:

```csharp
RegistrationResponse response = PasskeyJson.ReadRegistration(responseJson);
RegistrationResult result = passkeys.VerifyRegistration(response, originalOptions);
```

Insert the credential with a database-wide unique constraint on `CredentialId`. Save `CosePublicKey`, `UserHandle`, `SignatureCounter` and `IsBackupEligible`; preserve `AaGuid`, `IsBackedUp`, `AttestationFormat`, `Trust` and `Transports` as needed. Public keys remain COSE/CBOR bytes, including keys previously stored by Fido2. No database dependency or schema is imposed.

## Authentication

```csharp
AuthenticationOptions options = passkeys.CreateAuthenticationOptions(allowedCredentials);
string optionsJson = PasskeyJson.Serialize(options);
```

Omit `allowedCredentials` for username-less sign-in. For username-based sign-in, provide only credentials belonging to the requested account and bind the pending challenge to that account. Persist and expire authentication options separately from registration options. `PasskeyJson.ReadAuthenticationOptions` restores server state.

```javascript
const publicKey = PublicKeyCredential.parseRequestOptionsFromJSON(options);
const credential = await navigator.credentials.get({ publicKey });
const response = credential.toJSON();
```

Resolve the stored credential by its ID, consume the pending challenge once, and verify:

```csharp
AuthenticationResponse response = PasskeyJson.ReadAuthentication(responseJson);
StoredCredential stored = new(credentialId, cosePublicKey, userHandle,
    signatureCounter, isBackupEligible);
AuthenticationResult result = passkeys.VerifyAuthentication(response, originalOptions, stored);
```

The returned credential ID must resolve to the expected application account. Update the counter and backup state atomically against the stored version before issuing a session. Zero counters are accepted when both old and new values are zero. Otherwise the counter must increase. Backup eligibility cannot change; backup state can change. Username-less responses must contain the stored user handle. If a username-based response contains a user handle, it must also match.

## Attestation policy

Supported formats: `none`, `packed` (self or certificate), `fido-u2f`, `apple`, `android-key`, and TPM 2.0 certificate attestation. Signatures and format-specific bindings are checked. ECDAA, Android SafetyNet, TPM SHA-1 names and unknown formats are rejected. Cross-origin/embedded ceremonies, legacy AppID, related-origin discovery, extension output APIs and metadata-service integration are not implemented.

Requesting direct attestation does not establish manufacturer trust. By default a valid certificate statement without a configured trust anchor is returned as `AttestationTrust.Untrusted`; `none` and self attestation have separate result values. Applications requiring an attested device must configure trusted DER root certificates and set `RequireTrustedAttestation = true`. This rejects untrusted, self and absent attestation. Trust roots must come from an authenticated source outside the response.

Chain validation uses only supplied certificates and configured roots, disables certificate downloads, and does not perform online revocation checks. The application owns current device trust, revocation and metadata policy. Android Key authorization is checked across software and hardware authorization lists; this does not enforce a hardware-only key policy.

## Application responsibilities and errors

The four ceremony operations are synchronous, stateless CPU operations. Register the server as a singleton if desired. Database and challenge-store operations belong to the application and should use their asynchronous APIs.

Always use the original server-owned options. The browser's `timeout` is a hint, not server-side expiry. Verification does not consume challenges, enforce credential uniqueness in storage, select an account, authorize a registration, or create a session. These operations must fail closed and complete before sign-in succeeds.

Reject responses when verification throws. Protocol policy failures use `PasskeyVerificationException`; malformed JSON, CBOR, ASN.1 and cryptographic material can also raise their .NET parsing or cryptography exceptions. Apply request-body limits before deserializing and never return parsing internals to the client.

Automated tests cover complete ceremonies, supported signature algorithms, certificate formats, trust roots, malformed responses and binding failures. Ed25519 has RFC 8032 vectors and a separately signed assertion. A Chromium virtual authenticator registration and assertion and 210 Wycheproof Ed25519 vectors were also checked during implementation. Physical-device interoperability, Windows/macOS execution, external security review and FIDO certification remain open.
