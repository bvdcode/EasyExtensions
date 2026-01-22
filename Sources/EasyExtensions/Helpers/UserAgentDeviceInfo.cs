// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.Helpers
{
    /// <summary>
    /// Represents device information parsed from a user agent string, including device type, model, and a user-friendly
    /// name.
    /// </summary>
    public sealed class UserAgentDeviceInfo
    {
        /// <summary>
        /// Gets the type of device identified by the user agent string.
        /// </summary>
        public UserAgentDeviceType Type { get; }

        /// <summary>
        /// Gets the name or identifier of the model associated with this instance.
        /// </summary>
        public string? Model { get; }

        /// <summary>
        /// Gets the user-friendly display name associated with the object.
        /// </summary>
        public string? FriendlyName { get; }

        /// <summary>
        /// Initializes a new instance of the UserAgentDeviceInfo class with the specified device type, model, and
        /// friendly name.
        /// </summary>
        /// <param name="type">The type of device represented by this instance.</param>
        /// <param name="model">The device model name, or null if not specified.</param>
        /// <param name="friendlyName">A user-friendly name for the device, or null if not specified.</param>
        public UserAgentDeviceInfo(UserAgentDeviceType type, string? model = null, string? friendlyName = null)
        {
            Type = type;
            Model = model;
            FriendlyName = friendlyName;
        }
    }
}
