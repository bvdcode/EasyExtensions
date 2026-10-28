// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Passkeys
{
    public class PasskeyVerificationException(string message) : Exception(message);
}
