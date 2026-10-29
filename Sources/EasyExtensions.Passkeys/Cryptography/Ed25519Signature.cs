// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Numerics;
using System.Security.Cryptography;

namespace EasyExtensions.Passkeys.Cryptography
{
    /// <summary>Strict Ed25519 verification of public data. Does not generate keys or sign messages.</summary>
    public static class Ed25519Signature
    {
        private static readonly BigInteger Prime = (BigInteger.One << 255) - 19;
        private static readonly BigInteger Order = (BigInteger.One << 252) + BigInteger.Parse("27742317777372353535851937790883648493");
        private static readonly BigInteger D = Mod(-121665 * BigInteger.ModPow(121666, Prime - 2, Prime));
        private static readonly BigInteger SquareRootOfMinusOne = BigInteger.ModPow(2, (Prime - 1) / 4, Prime);
        private static readonly EdwardsPoint Identity = new(0, 1, 1, 0);
        private static readonly EdwardsPoint BasePoint = DecodeBasePoint();

        internal static bool IsValidPublicKey(ReadOnlySpan<byte> publicKey)
        {
            return publicKey.Length == 32 && TryDecode(publicKey, out EdwardsPoint key)
                && !Equal(key, Identity) && Equal(Multiply(key, Order), Identity);
        }

        public static bool Verify(ReadOnlySpan<byte> publicKey, ReadOnlySpan<byte> message, ReadOnlySpan<byte> signature)
        {
            if (publicKey.Length != 32 || signature.Length != 64)
            {
                return false;
            }
            BigInteger scalar = new(signature[32..], isUnsigned: true, isBigEndian: false);
            if (scalar >= Order || !TryDecode(publicKey, out EdwardsPoint key) || !TryDecode(signature[..32], out EdwardsPoint r))
            {
                return false;
            }
            if (Equal(key, Identity) || Equal(r, Identity)
                || !Equal(Multiply(key, Order), Identity) || !Equal(Multiply(r, Order), Identity))
            {
                return false;
            }
            using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
            hash.AppendData(signature[..32]);
            hash.AppendData(publicKey);
            hash.AppendData(message);
            BigInteger challenge = new(hash.GetHashAndReset(), isUnsigned: true, isBigEndian: false);
            challenge %= Order;
            return Equal(Multiply(BasePoint, scalar), Add(r, Multiply(key, challenge)));
        }

        private static bool TryDecode(ReadOnlySpan<byte> encoded, out EdwardsPoint point)
        {
            point = Identity;
            byte[] buffer = encoded.ToArray();
            int sign = buffer[31] >> 7;
            buffer[31] &= 0x7f;
            BigInteger y = new(buffer, isUnsigned: true, isBigEndian: false);
            if (y >= Prime)
            {
                return false;
            }
            BigInteger ySquared = Mod(y * y);
            BigInteger u = Mod(ySquared - 1);
            BigInteger v = Mod((D * ySquared) + 1);
            BigInteger xSquared = Mod(u * BigInteger.ModPow(v, Prime - 2, Prime));
            BigInteger x = BigInteger.ModPow(xSquared, (Prime + 3) / 8, Prime);
            if (Mod((x * x) - xSquared) != 0)
            {
                x = Mod(x * SquareRootOfMinusOne);
            }
            if (Mod((x * x) - xSquared) != 0 || (x.IsZero && sign != 0))
            {
                return false;
            }
            if ((x.IsEven ? 0 : 1) != sign)
            {
                x = Prime - x;
            }
            point = new(x, y, 1, Mod(x * y));
            return true;
        }

        private static EdwardsPoint Add(EdwardsPoint left, EdwardsPoint right)
        {
            BigInteger a = Mod((left.Y - left.X) * (right.Y - right.X));
            BigInteger b = Mod((left.Y + left.X) * (right.Y + right.X));
            BigInteger c = Mod(2 * D * left.T * right.T);
            BigInteger d = Mod(2 * left.Z * right.Z);
            BigInteger e = Mod(b - a);
            BigInteger f = Mod(d - c);
            BigInteger g = Mod(d + c);
            BigInteger h = Mod(b + a);
            return new(Mod(e * f), Mod(g * h), Mod(f * g), Mod(e * h));
        }

        private static EdwardsPoint Multiply(EdwardsPoint point, BigInteger scalar)
        {
            EdwardsPoint result = Identity;
            while (scalar > 0)
            {
                if (!scalar.IsEven)
                {
                    result = Add(result, point);
                }
                point = Add(point, point);
                scalar >>= 1;
            }
            return result;
        }

        private static bool Equal(EdwardsPoint left, EdwardsPoint right)
        {
            return Mod((left.X * right.Z) - (right.X * left.Z)).IsZero
                && Mod((left.Y * right.Z) - (right.Y * left.Z)).IsZero;
        }

        private static BigInteger Mod(BigInteger value)
        {
            return ((value % Prime) + Prime) % Prime;
        }

        private static EdwardsPoint DecodeBasePoint()
        {
            byte[] encoded = Convert.FromHexString("5866666666666666666666666666666666666666666666666666666666666666");
            if (!TryDecode(encoded, out EdwardsPoint point))
            {
                throw new InvalidOperationException("Invalid Ed25519 base point.");
            }
            return point;
        }
    }
}
