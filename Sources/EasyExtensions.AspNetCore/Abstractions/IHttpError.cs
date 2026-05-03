// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace EasyExtensions.AspNetCore.Abstractions
{
    /// <summary>
    /// Interface for HTTP error.
    /// </summary>
    public interface IHttpError
    {
        /// <summary>
        /// Gets error model.
        /// </summary>
        /// <param name="traceId">Optional trace ID to include in the error model.</param>
        /// <param name="path">Optional request path to include in the error model.</param>
        /// <returns> Error model. </returns>
        ProblemDetails GetErrorModel(string? traceId = null, string? path = null);

        /// <summary>
        /// Gets status code.
        /// </summary>
        HttpStatusCode StatusCode { get; }
    }
}
