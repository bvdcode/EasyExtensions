// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.AspNetCore.Abstractions;
using EasyExtensions.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
    /// <param name="extra">Optional additional error details. This parameter can be used to provide extra information about the error, such as specific permission
    /// requirements, user roles, or any other relevant context that may help in understanding the access denial.</param>
    public class WebApiException(
        HttpStatusCode statusCode,
        string objectName,
        string message,
        object? extra = null)
            : Exception(message), IHttpError
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
        /// Additional error details. This property can be used to provide extra 
        /// information about the error, such as validation errors, stack traces, or any
        /// other relevant context that may help in understanding the error.
        /// </summary>
        public object? Extra { get; } = extra;

        /// <summary>
        /// Get error model.
        /// </summary>
        /// <returns> Error model. </returns>
        public ProblemDetails GetErrorModel(string? traceId = null, string? path = null)
        {
            if (!string.IsNullOrWhiteSpace(ObjectName))
            {
                var details = new ValidationProblemDetails
                {
                    Detail = this.Message,
                    Instance = path ?? "/",
                    Status = (int)StatusCode,
                    Type = GetRfcType(StatusCode),
                    Errors = new Dictionary<string, string[]>(),
                    Extensions = new Dictionary<string, object?>(),
                    Title = ReasonPhrases.GetReasonPhrase((int)StatusCode),
                };
                details.Extensions["traceId"] = traceId ?? Activity.Current?.Id ?? "-";
                ControllerBaseExtensions.AddExtra(details, Extra);
                details.Errors[ObjectName] = [Message];

                return details;
            }

            var problemDetails = new ProblemDetails
            {
                Detail = this.Message,
                Instance = path ?? "/",
                Status = (int)StatusCode,
                Type = GetRfcType(StatusCode),
                Extensions = new Dictionary<string, object?>(),
                Title = ReasonPhrases.GetReasonPhrase((int)StatusCode),
            };
            problemDetails.Extensions["traceId"] = traceId ?? Activity.Current?.Id ?? "-";
            ControllerBaseExtensions.AddExtra(problemDetails, Extra);

            return problemDetails;
        }

        private static string GetRfcType(HttpStatusCode statusCode)
            => statusCode switch
            {
                HttpStatusCode.BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                HttpStatusCode.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
                HttpStatusCode.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                HttpStatusCode.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                HttpStatusCode.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                HttpStatusCode.UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
                HttpStatusCode.TooManyRequests => "https://tools.ietf.org/html/rfc6585#section-4",
                HttpStatusCode.InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                HttpStatusCode.ServiceUnavailable => "https://tools.ietf.org/html/rfc7231#section-6.6.4",
                _ => "https://tools.ietf.org/html/rfc7231"
            };
    }
}
