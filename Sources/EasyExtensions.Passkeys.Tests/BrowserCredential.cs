// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Buffers.Binary;
using System.Formats.Cbor;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using EasyExtensions.Passkeys.Models;
using EasyExtensions.Passkeys.Serialization;

namespace EasyExtensions.Passkeys.Tests
{
    internal class BrowserCredential : IDisposable
    {
        internal const string Origin = "https://login.example.com";
        internal static readonly PasskeyUser User = new([1, 2, 3, 4], "user", "User");
        private readonly ECDsa? ec;
        private readonly RSA? rsa;
        internal CoseAlgorithm Algorithm { get; }
        internal byte[] Id { get; } = RandomNumberGenerator.GetBytes(32);
        internal Guid AaGuid { get; set; } = Guid.NewGuid();
        internal byte[] PublicKey { get; }

        internal BrowserCredential(CoseAlgorithm algorithm = CoseAlgorithm.ES256)
        {
            Algorithm = algorithm;
            switch (algorithm)
            {
                case CoseAlgorithm.ES256:
                    ec = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                    break;
                case CoseAlgorithm.ES384:
                    ec = ECDsa.Create(ECCurve.NamedCurves.nistP384);
                    break;
                case CoseAlgorithm.ES512:
                    ec = ECDsa.Create(ECCurve.NamedCurves.nistP521);
                    break;
                case CoseAlgorithm.ES256K:
                    ec = ECDsa.Create(ECCurve.CreateFromFriendlyName("secP256k1"));
                    break;
                case CoseAlgorithm.RS256:
                case CoseAlgorithm.RS384:
                case CoseAlgorithm.RS512:
                case CoseAlgorithm.PS256:
                case CoseAlgorithm.PS384:
                case CoseAlgorithm.PS512:
                    rsa = RSA.Create(2048);
                    break;
                default:
                    throw new ArgumentException("Unsupported test credential algorithm.", nameof(algorithm));
            }
            PublicKey = EncodeKey();
        }

        internal static PasskeyServerOptions Configuration => new("example.com", "Example", [Origin]);
        internal static PasskeyServer Server() => new(Configuration);

        internal ECParameters EcParameters() => (ec ?? throw new InvalidOperationException()).ExportParameters(false);
        internal RSAParameters RsaParameters() => (rsa ?? throw new InvalidOperationException()).ExportParameters(false);

        internal CertificateRequest CertificateRequest(string subject)
        {
            if (ec is not null)
            {
                return new CertificateRequest(subject, ec, HashAlgorithmName.SHA256);
            }
            return new CertificateRequest(subject, rsa ?? throw new InvalidOperationException(), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        internal RegistrationResponse Register(RegistrationOptions options, string format = "packed", byte flags = 0x45,
            uint counter = 0, string origin = Origin, byte[]? statement = null)
        {
            byte[] client = ClientData("webauthn.create", options.Challenge, origin);
            byte[] data = AuthenticatorData(flags, counter, registration: true);
            statement ??= format switch
            {
                "none" => EmptyMap(),
                "packed" => PackedStatement(Sign([.. data, .. SHA256.HashData(client)])),
                _ => EmptyMap()
            };
            byte[] attestation = Attestation(format, data, statement);
            return new RegistrationResponse(Base64UrlConverter.Encode(Id), Id, "public-key", new AttestationResponse(client, attestation));
        }

        internal AuthenticationResponse Authenticate(AuthenticationOptions options, byte flags = 0x05, uint counter = 0, string origin = Origin)
        {
            byte[] client = ClientData("webauthn.get", options.Challenge, origin);
            byte[] data = AuthenticatorData(flags, counter, registration: false);
            byte[] signature = Sign([.. data, .. SHA256.HashData(client)]);
            return new AuthenticationResponse(Base64UrlConverter.Encode(Id), Id, "public-key",
                new AssertionResponse(client, data, signature) { UserHandle = User.Id });
        }

        internal StoredCredential Stored(uint counter = 0, bool backupEligible = false)
        {
            return new StoredCredential(Id, PublicKey, User.Id, counter, backupEligible);
        }

        internal byte[] AuthenticatorData(byte flags, uint counter, bool registration)
        {
            byte[] header = new byte[37];
            SHA256.HashData(Encoding.UTF8.GetBytes("example.com")).CopyTo(header, 0);
            header[32] = flags;
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(33), counter);
            if (!registration)
            {
                return header;
            }
            byte[] attested = new byte[18];
            AaGuid.TryWriteBytes(attested.AsSpan(0, 16), bigEndian: true, out _);
            BinaryPrimitives.WriteUInt16BigEndian(attested.AsSpan(16), (ushort)Id.Length);
            return [.. header, .. attested, .. Id, .. PublicKey];
        }

        internal byte[] Sign(byte[] data)
        {
            HashAlgorithmName hash = Algorithm switch
            {
                CoseAlgorithm.ES256 or CoseAlgorithm.ES256K or CoseAlgorithm.RS256 or CoseAlgorithm.PS256 => HashAlgorithmName.SHA256,
                CoseAlgorithm.ES384 or CoseAlgorithm.RS384 or CoseAlgorithm.PS384 => HashAlgorithmName.SHA384,
                CoseAlgorithm.ES512 or CoseAlgorithm.RS512 or CoseAlgorithm.PS512 => HashAlgorithmName.SHA512,
                _ => throw new ArgumentException("Unsupported test hash.")
            };
            if (ec is not null)
            {
                return ec.SignData(data, hash, DSASignatureFormat.Rfc3279DerSequence);
            }
            RSASignaturePadding padding = Algorithm switch
            {
                CoseAlgorithm.RS256 or CoseAlgorithm.RS384 or CoseAlgorithm.RS512 => RSASignaturePadding.Pkcs1,
                CoseAlgorithm.PS256 or CoseAlgorithm.PS384 or CoseAlgorithm.PS512 => RSASignaturePadding.Pss,
                _ => throw new ArgumentException("Unsupported test RSA padding.")
            };
            return (rsa ?? throw new InvalidOperationException()).SignData(data, hash, padding);
        }

        internal byte[] PackedStatement(byte[] signature, params byte[][] certificates)
        {
            CborWriter writer = new();
            writer.WriteStartMap(certificates.Length == 0 ? 2 : 3);
            writer.WriteTextString("alg");
            writer.WriteInt32((int)Algorithm);
            writer.WriteTextString("sig");
            writer.WriteByteString(signature);
            if (certificates.Length > 0)
            {
                writer.WriteTextString("x5c");
                writer.WriteStartArray(certificates.Length);
                foreach (byte[] certificate in certificates)
                {
                    writer.WriteByteString(certificate);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndMap();
            return writer.Encode();
        }

        internal static byte[] Attestation(string format, byte[] data, byte[] statement)
        {
            CborWriter writer = new();
            writer.WriteStartMap(3);
            writer.WriteTextString("fmt");
            writer.WriteTextString(format);
            writer.WriteTextString("authData");
            writer.WriteByteString(data);
            writer.WriteTextString("attStmt");
            writer.WriteEncodedValue(statement);
            writer.WriteEndMap();
            return writer.Encode();
        }

        internal static byte[] ClientData(string operation, byte[] challenge, string origin, bool crossOrigin = false)
        {
            using MemoryStream stream = new();
            using (Utf8JsonWriter writer = new(stream))
            {
                writer.WriteStartObject();
                writer.WriteString("type", operation);
                writer.WriteString("challenge", Base64UrlConverter.Encode(challenge));
                writer.WriteString("origin", origin);
                writer.WriteBoolean("crossOrigin", crossOrigin);
                writer.WriteEndObject();
            }
            return stream.ToArray();
        }

        private byte[] EncodeKey()
        {
            CborWriter writer = new();
            if (ec is not null)
            {
                ECParameters parameters = ec.ExportParameters(false);
                int curve = Algorithm switch
                {
                    CoseAlgorithm.ES256 => 1,
                    CoseAlgorithm.ES384 => 2,
                    CoseAlgorithm.ES512 => 3,
                    CoseAlgorithm.ES256K => 8,
                    _ => throw new ArgumentException("Unsupported test curve.")
                };
                writer.WriteStartMap(5);
                writer.WriteInt32(1);
                writer.WriteInt32(2);
                writer.WriteInt32(3);
                writer.WriteInt32((int)Algorithm);
                writer.WriteInt32(-1);
                writer.WriteInt32(curve);
                writer.WriteInt32(-2);
                writer.WriteByteString(parameters.Q.X!);
                writer.WriteInt32(-3);
                writer.WriteByteString(parameters.Q.Y!);
            }
            else
            {
                RSAParameters parameters = (rsa ?? throw new InvalidOperationException()).ExportParameters(false);
                writer.WriteStartMap(4);
                writer.WriteInt32(1);
                writer.WriteInt32(3);
                writer.WriteInt32(3);
                writer.WriteInt32((int)Algorithm);
                writer.WriteInt32(-1);
                writer.WriteByteString(parameters.Modulus!);
                writer.WriteInt32(-2);
                writer.WriteByteString(parameters.Exponent!);
            }
            writer.WriteEndMap();
            return writer.Encode();
        }

        private static byte[] EmptyMap() => [0xA0];

        public void Dispose()
        {
            ec?.Dispose();
            rsa?.Dispose();
        }
    }
}
