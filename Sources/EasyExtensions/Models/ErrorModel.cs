// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Collections.Generic;

namespace EasyExtensions.Models
{
    /// <summary>
    /// Error model.
    /// </summary>
    public class ErrorModel
    {
        /// <summary>
        /// URI of the error type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Error title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Error status code.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Trace ID.
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Error details.
        /// </summary>
        public IDictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}
