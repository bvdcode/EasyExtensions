// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.Abstractions;
using EasyExtensions.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown to indicate an error response from a Web API, including an associated
    /// HTTP status code and object name.
    /// </summary>
    /// <remarks>Use this exception to return detailed error information from Web API actions, including a
    /// status code and object-specific error details. The exception can be used to generate standardized error
    /// responses for clients.</remarks>
    /// <param name="statusCode">The HTTP status code to associate with the exception. Indicates the nature of the error as defined by the HTTP
    /// protocol.</param>
    /// <param name="objectName">The name of the object or entity related to the error. Used to identify the source of the error in the response.</param>
    /// <param name="message">The error message that describes the reason for the exception.</param>
    public class WebApiException(HttpStatusCode statusCode, string objectName, string message) : Exception(message), IHttpError
    {
        /// <summary>
        /// HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; } = statusCode;

        /// <summary>
        /// Object name.
        /// </summary>
        public string ObjectName { get; } = objectName;

        /// <summary>
        /// Get error model.
        /// </summary>
        /// <returns> Error model. </returns>
        public ErrorModel GetErrorModel()
        {
            return new()
            {
                Status = (int)StatusCode,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Web API actions errors occurred.",
                TraceId = Activity.Current?.Id ?? "-",
                Errors = new Dictionary<string, string>
                {
                    { ObjectName, Message }
                }
            };
        }
    }
}
