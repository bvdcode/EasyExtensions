// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Extensions;
using System.Text;

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

        /// <summary>
        /// Returns a string that represents the current object, including its type, model, and friendly name.
        /// </summary>
        /// <returns>A string containing the type, model, and friendly name of the object. If the model or friendly name is not
        /// set, "Unknown Model" or "Unknown Device" is used, respectively.</returns>
        override public string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Type.ToNiceString());
            if (!string.IsNullOrEmpty(Model))
            {
                sb.Append($" ({Model})");
            }
            if (!string.IsNullOrEmpty(FriendlyName))
            {
                sb.Append($": {FriendlyName}");
            }
            return sb.ToString();
        }
    }
}
