// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Text.Json.Serialization;

namespace EasyExtensions.AspNetCore.Authorization.Models.Dto
{
    /// <summary>
    /// Represents the user information returned from a Google OpenID Connect authentication response.
    /// </summary>
    /// <remarks>This data transfer object maps to the standard claims provided by Google when authenticating
    /// users via OpenID Connect. It is typically used to deserialize the JSON payload received from Google's identity
    /// platform after a successful authentication. Property values correspond to the claims defined in the OpenID
    /// Connect specification as implemented by Google.</remarks>
    public class GoogleOpenIdResponseDto
    {
        /// <summary>
        /// Gets or sets the subject identifier for the entity represented by the token.
        /// </summary>
        [JsonPropertyName("sub")]
        public string Subject { get; set; } = null!;

        /// <summary>
        /// Gets or sets the name associated with this instance.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the given name (first name) of the person.
        /// </summary>
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the family name (surname) of the person.
        /// </summary>
        [JsonPropertyName("family_name")]
        public string FamilyName { get; set; } = null!;

        /// <summary>
        /// Gets or sets the URL of the user's profile picture.
        /// </summary>
        [JsonPropertyName("picture")]
        public string PictureUrl { get; set; } = null!;

        /// <summary>
        /// Gets or sets the email address associated with the user.
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the user's email address has been verified.
        /// </summary>
        [JsonPropertyName("email_verified")]
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Gets or sets the hosted domain associated with the authenticated user, if available.
        /// </summary>
        /// <remarks>This property is typically present when the user's account is part of a hosted
        /// domain, such as a Google Workspace organization. If the user does not belong to a hosted domain, this
        /// property may be null.</remarks>
        [JsonPropertyName("hd")]
        public string? HostedDomain { get; set; } = null!;
    }
}
