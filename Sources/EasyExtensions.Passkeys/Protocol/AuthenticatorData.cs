// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Binary;
using System.Formats.Cbor;
using System.Security.Cryptography;
using System.Text;
using EasyExtensions.Passkeys.Cryptography;

namespace EasyExtensions.Passkeys.Protocol
{
    internal class AuthenticatorData
    {
        internal uint SignatureCounter { get; }
        internal bool IsBackupEligible { get; }
        internal bool IsBackedUp { get; }
        internal Guid AaGuid { get; }
        internal byte[] CredentialId { get; } = [];
        internal byte[] PublicKey { get; } = [];
        internal CoseKey? Key { get; }

        internal AuthenticatorData(byte[] data, string relyingPartyId, bool registration, bool requireVerification)
        {
            ArgumentNullException.ThrowIfNull(data);
            if (data.Length is < 37 or > ProtocolLimits.MaximumAttestationLength)
            {
                throw new PasskeyVerificationException("Invalid authenticator data size.");
            }
            byte[] expectedRpHash = SHA256.HashData(Encoding.UTF8.GetBytes(relyingPartyId));
            byte flags = data[32];
            if (!CryptographicOperations.FixedTimeEquals(data.AsSpan(0, 32), expectedRpHash)
                || (flags & 0x01) == 0 || (requireVerification && (flags & 0x04) == 0))
            {
                throw new PasskeyVerificationException("RP ID, user presence or user verification does not match.");
            }
            IsBackupEligible = (flags & 0x08) != 0;
            IsBackedUp = (flags & 0x10) != 0;
            if ((flags & 0x22) != 0 || (IsBackedUp && !IsBackupEligible) || ((flags & 0x40) != 0) != registration)
            {
                throw new PasskeyVerificationException("Invalid authenticator flags.");
            }
            SignatureCounter = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(33, 4));
            int offset = 37;
            if (registration)
            {
                if (data.Length < 55)
                {
                    throw new PasskeyVerificationException("Missing attested credential data.");
                }
                AaGuid = new Guid(data.AsSpan(offset, 16), bigEndian: true);
                int idLength = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(53, 2));
                if (idLength is 0 or > ProtocolLimits.MaximumCredentialIdLength || data.Length < 55 + idLength)
                {
                    throw new PasskeyVerificationException("Invalid credential ID length.");
                }
                CredentialId = data.AsSpan(55, idLength).ToArray();
                offset = 55 + idLength;
                CborReader reader = new(data.AsMemory(offset), CborConformanceMode.Strict, allowMultipleRootLevelValues: true);
                PublicKey = reader.ReadEncodedValue().ToArray();
                Key = new CoseKey(PublicKey);
                offset += PublicKey.Length;
            }
            if ((flags & 0x80) != 0)
            {
                if (offset == data.Length)
                {
                    throw new PasskeyVerificationException("Missing authenticator extension data.");
                }
                _ = CborMap.Read(data.AsMemory(offset));
                offset = data.Length;
            }
            if (offset != data.Length)
            {
                throw new PasskeyVerificationException("Unexpected trailing authenticator data.");
            }
        }
    }
}
