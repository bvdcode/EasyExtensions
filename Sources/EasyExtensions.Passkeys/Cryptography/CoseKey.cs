// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Security.Cryptography;
using EasyExtensions.Passkeys.Models;
using EasyExtensions.Passkeys.Protocol;

namespace EasyExtensions.Passkeys.Cryptography
{
    internal class CoseKey
    {
        internal CoseAlgorithm Algorithm { get; }
        internal int Type { get; }
        internal int Curve { get; }
        internal byte[] X { get; }
        internal byte[] Y { get; }
        internal byte[] Modulus { get; }
        internal byte[] Exponent { get; }

        internal CoseKey(ReadOnlyMemory<byte> encoded)
        {
            if (encoded.Length is 0 or > ProtocolLimits.MaximumPublicKeyLength)
            {
                throw new PasskeyVerificationException("Invalid COSE key size.");
            }
            CborMap map = CborMap.Read(encoded, integerKeys: true);
            Type = map.Integer("1");
            Algorithm = (CoseAlgorithm)map.Integer("3");
            X = [];
            Y = [];
            Modulus = [];
            Exponent = [];
            if (map.Contains("-4") || (Type == 3 && Enumerable.Range(3, 7).Any(label => map.Contains("-" + label))))
            {
                throw new PasskeyVerificationException("Private COSE keys are not accepted.");
            }
            switch (Type)
            {
                case 2:
                    Curve = map.Integer("-1");
                    X = map.Bytes("-2");
                    Y = map.Bytes("-3");
                    int length = CoordinateLength(Algorithm, Curve);
                    if (X.Length != length || Y.Length != length)
                    {
                        throw new PasskeyVerificationException("Invalid EC coordinate length.");
                    }
                    using (ECDsa key = CreateECDsa())
                    {
                        key.ExportParameters(false);
                    }
                    break;
                case 3:
                    Modulus = map.Bytes("-1");
                    Exponent = map.Bytes("-2");
                    ValidateRsa();
                    break;
                case 1:
                    Curve = map.Integer("-1");
                    X = map.Bytes("-2");
                    if (Algorithm != CoseAlgorithm.EdDSA || Curve != 6 || !Ed25519Signature.IsValidPublicKey(X))
                    {
                        throw new PasskeyVerificationException("Only Ed25519 OKP keys are supported.");
                    }
                    break;
                default:
                    throw new PasskeyVerificationException("Unsupported COSE key type.");
            }
        }

        internal bool Verify(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
        {
            switch (Type)
            {
                case 2:
                    using (ECDsa key = CreateECDsa())
                    {
                        return key.VerifyData(data, signature, HashName(Algorithm), DSASignatureFormat.Rfc3279DerSequence);
                    }
                case 3:
                    using (RSA key = RSA.Create(new RSAParameters { Modulus = Modulus, Exponent = Exponent }))
                    {
                        return key.VerifyData(data, signature, HashName(Algorithm), RsaPadding(Algorithm));
                    }
                case 1:
                    return Ed25519Signature.Verify(X, data, signature);
                default:
                    throw new PasskeyVerificationException("Unsupported COSE key type.");
            }
        }

        internal ECDsa CreateECDsa()
        {
            ECCurve curve = (Algorithm, Curve) switch
            {
                (CoseAlgorithm.ES256, 1) => ECCurve.NamedCurves.nistP256,
                (CoseAlgorithm.ES384, 2) => ECCurve.NamedCurves.nistP384,
                (CoseAlgorithm.ES512, 3) => ECCurve.NamedCurves.nistP521,
                (CoseAlgorithm.ES256K, 8) => ECCurve.CreateFromFriendlyName("secP256k1"),
                _ => throw new PasskeyVerificationException("Algorithm and EC curve do not match.")
            };
            return ECDsa.Create(new ECParameters { Curve = curve, Q = new ECPoint { X = X, Y = Y } });
        }

        internal static HashAlgorithmName HashName(CoseAlgorithm algorithm)
        {
            return algorithm switch
            {
                CoseAlgorithm.ES256 or CoseAlgorithm.ES256K or CoseAlgorithm.RS256 or CoseAlgorithm.PS256 => HashAlgorithmName.SHA256,
                CoseAlgorithm.ES384 or CoseAlgorithm.RS384 or CoseAlgorithm.PS384 => HashAlgorithmName.SHA384,
                CoseAlgorithm.ES512 or CoseAlgorithm.RS512 or CoseAlgorithm.PS512 => HashAlgorithmName.SHA512,
                _ => throw new PasskeyVerificationException("Unsupported signature hash algorithm.")
            };
        }

        internal static RSASignaturePadding RsaPadding(CoseAlgorithm algorithm)
        {
            return algorithm switch
            {
                CoseAlgorithm.RS256 or CoseAlgorithm.RS384 or CoseAlgorithm.RS512 => RSASignaturePadding.Pkcs1,
                CoseAlgorithm.PS256 or CoseAlgorithm.PS384 or CoseAlgorithm.PS512 => RSASignaturePadding.Pss,
                _ => throw new PasskeyVerificationException("Algorithm cannot be used with RSA.")
            };
        }

        private void ValidateRsa()
        {
            _ = RsaPadding(Algorithm);
            if (Modulus.Length is < 256 or > 1024 || Modulus[0] < 0x80 || (Modulus[^1] & 1) == 0
                || Exponent.Length is < 1 or > 4 || Exponent[0] == 0 || (Exponent[^1] & 1) == 0
                || (Exponent.Length == 1 && Exponent[0] < 3))
            {
                throw new PasskeyVerificationException("Invalid RSA public key.");
            }
        }

        private static int CoordinateLength(CoseAlgorithm algorithm, int curve)
        {
            return (algorithm, curve) switch
            {
                (CoseAlgorithm.ES256, 1) or (CoseAlgorithm.ES256K, 8) => 32,
                (CoseAlgorithm.ES384, 2) => 48,
                (CoseAlgorithm.ES512, 3) => 66,
                _ => throw new PasskeyVerificationException("Algorithm and EC curve do not match.")
            };
        }
    }
}
