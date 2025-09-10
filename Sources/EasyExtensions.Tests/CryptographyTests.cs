using System;
using System.Text;
using EasyExtensions.Services;
using System.Security.Cryptography;
using System.Diagnostics;
using NUnit.Framework.Internal;

namespace EasyExtensions.Tests
{
    [TestFixture]
    internal class CryptographyTests
    {
        private const string ValidPepper = "this-is-a-valid-test-pepper-12345"; // >16 bytes

        [Test]
        public void HashIterationDuration_IsAtLeast200ms()
        {
            const int expectedMinDurationMs = 200; // 100 ms
            Stopwatch stopwatch = Stopwatch.StartNew();
            var svc = new Pbkdf2PasswordHashService(ValidPepper);
            var password = "SomePassword!";
            svc.Hash(password);
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(expectedMinDurationMs), 
                $"Hashing should take at least {expectedMinDurationMs} ms to be secure enough. " +
                $"If your hardware is very fast, consider increasing the default iteration count in the Pbkdf2PasswordHashService constructor.");
        }

        [Test]
        public void Hash_And_Verify_Success()
        {
            var svc = new Pbkdf2PasswordHashService(ValidPepper, version: 1, iterations: 5_000);
            var password = "SuperSecret!";

            var phc = svc.Hash(password);
            var ok = svc.Verify(password, phc, out var needsRehash);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok, Is.True);
                Assert.That(needsRehash, Is.False);
            }
        }

        [Test]
        public void Verify_WrongPassword_Fails()
        {
            var svc = new Pbkdf2PasswordHashService(ValidPepper, iterations: 3_000);
            var phc = svc.Hash("CorrectHorseBatteryStaple");

            var ok = svc.Verify("wrong-password", phc, out var needsRehash);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok, Is.False);
                Assert.That(needsRehash, Is.False);
            }
        }

        [Test]
        public void Verify_WrongPepper_Fails()
        {
            var svc1 = new Pbkdf2PasswordHashService(ValidPepper, iterations: 2_000);
            var svc2 = new Pbkdf2PasswordHashService(ValidPepper + "-different", iterations: 2_000);
            var password = "Pa$$w0rd";

            var phc = svc1.Hash(password);
            var ok = svc2.Verify(password, phc, out var needsRehash);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok, Is.False);
                Assert.That(needsRehash, Is.False);
            }
        }

        [Test]
        public void Hash_ProducesDifferentHashes_DueToRandomSalt()
        {
            var svc = new Pbkdf2PasswordHashService(ValidPepper, iterations: 2_000);
            var password = "same-password";

            var phc1 = svc.Hash(password);
            var phc2 = svc.Hash(password);

            Assert.That(phc1, Is.Not.EqualTo(phc2));
        }

        [Test]
        public void Hash_Format_IsValidPHC()
        {
            var iterations = 1_500;
            var svc = new Pbkdf2PasswordHashService(ValidPepper, version: 1, iterations: iterations);
            var phc = svc.Hash("abc");

            // $pbkdf2-sha256$v=1$i=ITER$SALT$HASH
            Assert.That(phc, Does.StartWith("$pbkdf2-sha256$v=1$i=" + iterations + "$"));

            var parts = phc.Split('$', StringSplitOptions.RemoveEmptyEntries);
            Assert.That(parts, Has.Length.EqualTo(5));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(parts[0], Is.EqualTo("pbkdf2-sha256"));
                Assert.That(parts[1], Does.StartWith("v="));
                Assert.That(parts[2], Does.StartWith("i="));
                // Salt and hash should be valid base64
                Assert.That(() => Convert.FromBase64String(parts[3]), Throws.Nothing);
                Assert.That(() => Convert.FromBase64String(parts[4]), Throws.Nothing);
            }
        }

        [Test]
        public void Verify_NeedsRehash_WhenVersionIncreased()
        {
            var svcV1 = new Pbkdf2PasswordHashService(ValidPepper, version: 1, iterations: 2_000);
            var phc = svcV1.Hash("pwd");

            var svcV2 = new Pbkdf2PasswordHashService(ValidPepper, version: 2, iterations: 2_000);
            var ok = svcV2.Verify("pwd", phc, out var needsRehash);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok, Is.True);
                Assert.That(needsRehash, Is.True);
            }
        }

        [Test]
        public void Verify_NeedsRehash_WhenIterationsIncreased()
        {
            var svcLow = new Pbkdf2PasswordHashService(ValidPepper, version: 1, iterations: 1_000);
            var phc = svcLow.Hash("pwd");

            var svcHigh = new Pbkdf2PasswordHashService(ValidPepper, version: 1, iterations: 2_000);
            var ok = svcHigh.Verify("pwd", phc, out var needsRehash);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(ok, Is.True);
                Assert.That(needsRehash, Is.True);
            }
        }

        [Test]
        public void Verify_NeedsRehash_WhenHashSizeDifferent()
        {
            // Arrange
            var password = "pwd";
            var iterations = 1_000;
            var svc = new Pbkdf2PasswordHashService(ValidPepper, version: 1, iterations: iterations);
            var phc = svc.Hash(password);

            var parts = phc.Split('$', StringSplitOptions.RemoveEmptyEntries);
            var salt = Convert.FromBase64String(parts[3]);

            // Re-derive the input the same way as service does
            var key = Encoding.UTF8.GetBytes(ValidPepper);
            var pwdBytes = Encoding.UTF8.GetBytes(password);
            using var h = new HMACSHA256(key);
            var input = h.ComputeHash(pwdBytes);

            using var pbkdf2 = new Rfc2898DeriveBytes(input, salt, iterations, HashAlgorithmName.SHA256);
            var shortHash = pbkdf2.GetBytes(16); // smaller than default 32
            var phcShort = "$pbkdf2-sha256$" + parts[1] + "$" + parts[2] + "$" + parts[3] + "$" + Convert.ToBase64String(shortHash);

            // Act
            var ok = svc.Verify(password, phcShort, out var needsRehash);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(ok, Is.True);
                Assert.That(needsRehash, Is.True);
            }
        }

        [Test]
        public void Constructor_InvalidPepper_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Pbkdf2PasswordHashService("   "));
            Assert.Throws<ArgumentException>(() => new Pbkdf2PasswordHashService("short-pepper")); // < 16 bytes
        }

        [Test]
        public void Constructor_InvalidParameters_Throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pbkdf2PasswordHashService(ValidPepper, version: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pbkdf2PasswordHashService(ValidPepper, iterations: 0));
        }

        [Test]
        public void Hash_NullOrWhitespacePassword_Throws()
        {
            var svc = new Pbkdf2PasswordHashService(ValidPepper);
            Assert.Throws<ArgumentNullException>(() => svc.Hash(null!));
            Assert.Throws<ArgumentNullException>(() => svc.Hash("   "));
        }

        [Test]
        public void Verify_NullOrWhitespacePassword_Throws()
        {
            var svc = new Pbkdf2PasswordHashService(ValidPepper, iterations: 1_000);
            var phc = svc.Hash("pwd");

            Assert.Throws<ArgumentNullException>(() => svc.Verify(null!, phc, out _));
            Assert.Throws<ArgumentNullException>(() => svc.Verify("   ", phc, out _));
        }

        [Test]
        public void Verify_InvalidPhc_ReturnsFalse()
        {
            var svc = new Pbkdf2PasswordHashService(ValidPepper, iterations: 1_000);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(svc.Verify("pwd", null!, out _), Is.False);
                Assert.That(svc.Verify("pwd", "", out _), Is.False);
                Assert.That(svc.Verify("pwd", "$wrongprefix$v=1$i=1$AAAA$BBBB", out _), Is.False);
                Assert.That(svc.Verify("pwd", "$pbkdf2-sha256$v=x$i=1$AAAA$BBBB", out _), Is.False);
                Assert.That(svc.Verify("pwd", "$pbkdf2-sha256$v=1$i=x$AAAA$BBBB", out _), Is.False);
                Assert.That(svc.Verify("pwd", "$pbkdf2-sha256$v=1$i=1$not-base64$still-not", out _), Is.False);
                Assert.That(svc.Verify("pwd", "$pbkdf2-sha256$v=1$i=1$AAAA", out _), Is.False); // not enough parts
            }
        }
    }
}
