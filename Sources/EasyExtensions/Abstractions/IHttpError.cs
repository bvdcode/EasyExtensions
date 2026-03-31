// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Models;
using System.Net;

namespace EasyExtensions.Abstractions
{
    /// <summary>
    /// Interface for HTTP error.
    /// </summary>
    public interface IHttpError
    {
        /// <summary>
        /// Gets error model.
        /// </summary>
        /// <returns> Error model. </returns>
        ErrorModel GetErrorModel();

        /// <summary>
        /// Sets trace identifier for the error. This method can be used to associate a specific trace identifier
        /// with the error, which can be useful for tracking and correlation purposes in logging and diagnostics.
        /// </summary>
        /// <param name="traceId">The trace identifier to associate with the error.</param>
        void SetTraceIdentifier(string traceId);

        /// <summary>
        /// Gets status code.
        /// </summary>
        HttpStatusCode StatusCode { get; }
    }
}
