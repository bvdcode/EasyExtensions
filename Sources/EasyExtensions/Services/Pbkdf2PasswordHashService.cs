// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Abstractions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace EasyExtensions.Services
{
    /// <summary>
    /// PBKDF2 password hashing service using HMAC-SHA256.
    /// </summary>
    public class Pbkdf2PasswordHashService : IPasswordHashService
    {
        /// <summary>
        /// The version of the password hashing algorithm used. Default is 1.
        /// </summary>
        public int PasswordHashVersion => _version;

        private const int _version = 1;
        private readonly string _pepper;
        private readonly int _iterations;

        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const string Prefix = "pbkdf2-sha256";
        private readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="pepper">A secret value that is used in addition to the password. Must be at least 16 bytes (UTF-8).</param>
        /// <param name="iterations">The number of iterations for the PBKDF2 algorithm. Must be greater than 0. Default is 310,000.</param>
        /// <exception cref="ArgumentException">Thrown when the pepper is null, whitespace, or less than 16 bytes (UTF-8).</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the version or iterations are less than 1.</exception>
        /// <remarks>
        /// The pepper should be a long, random string that is kept secret and not stored in the database.
        /// It is recommended to use a unique pepper for each application.
        /// The default number of iterations is set to 310,000 as of 2025, which is a good balance between security and performance.
        /// Consider increasing this value as hardware capabilities improve over time.
        /// </remarks>
        public Pbkdf2PasswordHashService(string pepper, int iterations = 310_000)
        {
            if (string.IsNullOrWhiteSpace(pepper))
            {
                throw new ArgumentException("Pepper cannot be null or whitespace.", nameof(pepper));
            }

            if (Encoding.UTF8.GetByteCount(pepper) < 16)
            {
                throw new ArgumentException("Pepper must be at least 16 bytes (UTF-8).", nameof(pepper));
            }

            if (iterations < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be greater than 0.");
            }

            _pepper = pepper;
            _iterations = iterations;
        }

        /// <summary>
        /// Creates a password hash in PHC format like:
        /// <c>$pbkdf2-sha256$v=1$i=310000$[saltB64]$[hashB64]</c>
        /// </summary>
        /// <remarks>
        /// Default iterations: 310,000 (2025 OWASP PBKDF2-HMAC-SHA256 guidance). <br/>
        /// Adjust to target ~200–300 ms per hash on production hardware. <br/>
        /// </remarks>
        public string Hash(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] saltBytes = new byte[SaltSize];
            RandomNumberGenerator.Fill(saltBytes);
            var input = DeriveInput(password, _pepper);
            using var pbkdf2 = new Rfc2898DeriveBytes(input, saltBytes, _iterations, _hashAlgorithm);
            var hash = pbkdf2.GetBytes(HashSize);
            string hashB64 = Convert.ToBase64String(hash);
            string saltB64 = Convert.ToBase64String(saltBytes);
            return "$" + Prefix + "$v=" + _version + "$i=" + _iterations + "$" + saltB64 + "$" + hashB64;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// needsRehash becomes true when: <br/>
        /// - stored version &lt; current version <br/>
        /// - stored iterations &lt; current configured iterations <br/>
        /// - salt length != 16 <br/>
        /// - hash length != 32 <br/>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the password is null or whitespace.</exception>
        public bool Verify(string password, string passwordHash) => Verify(password, passwordHash, out _);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// needsRehash becomes true when: <br/>
        /// - stored version &lt; current version <br/>
        /// - stored iterations &lt; current configured iterations <br/>
        /// - salt length != 16 <br/>
        /// - hash length != 32 <br/>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the password is null or whitespace.</exception>
        public bool Verify(string password, string phc, out bool needsRehash)
        {
            needsRehash = false;
            if (string.IsNullOrWhiteSpace(phc))
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            if (!TryParsePhc(phc, out var parsed))
            {
                return false;
            }

            byte[] input = DeriveInput(password, _pepper);
            using var pbkdf2 = new Rfc2898DeriveBytes(input, parsed.Salt, parsed.Iterations, _hashAlgorithm);
            byte[] actual = pbkdf2.GetBytes(parsed.Hash.Length);

            var ok = CryptographicOperations.FixedTimeEquals(actual, parsed.Hash);
            if (ok && ShouldRehash(parsed))
            {
                needsRehash = true;
            }

            return ok;
        }

        private static byte[] DeriveInput(string password, string pepper)
        {
            byte[] pwdBytes = Encoding.UTF8.GetBytes(password);
            byte[] pepperBytes = Encoding.UTF8.GetBytes(pepper);
            try
            {
                using var hmac = new HMACSHA256(pepperBytes);
                return hmac.ComputeHash(pwdBytes);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(pwdBytes);
                CryptographicOperations.ZeroMemory(pepperBytes);
            }
        }

        private readonly struct ParsedPhc
        {
            public ParsedPhc(int version, int iterations, byte[] salt, byte[] hash)
            {
                Version = version;
                Iterations = iterations;
                Salt = salt;
                Hash = hash;
            }
            public int Version { get; }
            public int Iterations { get; }
            public byte[] Salt { get; }
            public byte[] Hash { get; }
        }

        private static bool TryParsePhc(string phc, out ParsedPhc parsed)
        {
            parsed = default;
            string[] parts = phc.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5 || parts[0] != Prefix)
            {
                return false;
            }

            // v=...
            if (!parts[1].StartsWith("v=") || !int.TryParse(parts[1][2..], out var version))
            {
                return false;
            }

            // i=...
            if (!parts[2].StartsWith("i=") || !int.TryParse(parts[2][2..], out var iterations))
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[3]);
                var hash = Convert.FromBase64String(parts[4]);
                parsed = new ParsedPhc(version, iterations, salt, hash);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private bool ShouldRehash(ParsedPhc p)
        {
            return p.Version < _version || p.Iterations < _iterations || p.Salt.Length != SaltSize || p.Hash.Length != HashSize;
        }
    }
}
