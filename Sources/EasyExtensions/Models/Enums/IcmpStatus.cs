// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Models.Enums
{
    /// <summary>
    /// Status of an ICMP ping operation.
    /// </summary>
    public enum IcmpStatus
    {
        /// <summary>
        /// The ping was successful.
        /// </summary>
        Success,

        /// <summary>
        /// The ping timed out.
        /// </summary>
        Timeout,

        /// <summary>
        /// An error occurred during the ping operation.
        /// </summary>
        Failed
    }
}
