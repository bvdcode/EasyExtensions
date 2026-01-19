// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Tests
{
    public class MathTests
    {
        [Test]
        public void Pow_ValidInput_ValidOutput()
        {
            Assert.That(2.Pow(8), Is.EqualTo(byte.MaxValue + 1));
        }

        /// <summary>
        /// Throw if calculation result more than Int32 (2^31 - 1) max value.
        /// </summary>
        [Test]
        public void Pow_ValidInput_ThrowOverflowException()
        {
            Assert.Throws<OverflowException>(() => 2.Pow(31));
        }
    }
}
