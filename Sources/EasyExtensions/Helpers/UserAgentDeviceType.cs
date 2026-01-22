// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// Specifies the type of device identified from a user agent string.
    /// </summary>
    /// <remarks>
    /// This enumeration is typically used to classify client devices based on their user agent information, such as
    /// distinguishing between mobile phones, tablets, desktop computers, and automated agents like bots or scripts.
    /// The values can be used to tailor application behavior or analytics based on the detected device type.
    /// </remarks>
    public enum UserAgentDeviceType
    {
        /// <summary>
        /// The device type cannot be determined from the available user agent information.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// An automated agent (crawler/indexer) identified as a bot.
        /// </summary>
        Bot,

        /// <summary>
        /// An automated client (for example a scriptable HTTP client) that is not classified as a bot.
        /// </summary>
        Script,

        /// <summary>
        /// A smart TV device.
        /// </summary>
        SmartTv,

        /// <summary>
        /// A game console device.
        /// </summary>
        GameConsole,

        /// <summary>
        /// An Apple iPhone device.
        /// </summary>
        IPhone,

        /// <summary>
        /// An Apple iPad device.
        /// </summary>
        IPad,

        /// <summary>
        /// An Apple iPod device.
        /// </summary>
        IPod,

        /// <summary>
        /// An Android-based phone device.
        /// </summary>
        AndroidPhone,

        /// <summary>
        /// An Android-based tablet device.
        /// </summary>
        AndroidTablet,

        /// <summary>
        /// A ChromeOS device (Chromebook).
        /// </summary>
        Chromebook,

        /// <summary>
        /// A Windows desktop or laptop computer.
        /// </summary>
        WindowsPc,

        /// <summary>
        /// A macOS desktop or laptop computer.
        /// </summary>
        Mac,

        /// <summary>
        /// A Linux desktop or laptop computer.
        /// </summary>
        LinuxPc,

        /// <summary>
        /// A generic mobile device when a more specific classification is not available.
        /// </summary>
        Mobile,

        /// <summary>
        /// A server-side or non-interactive client (for example, backend-to-backend traffic) when recognizable.
        /// </summary>
        Server,
    }
}