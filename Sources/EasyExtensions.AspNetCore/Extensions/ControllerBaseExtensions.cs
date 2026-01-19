// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;

namespace EasyExtensions.AspNetCore.Extensions
{
    /// <summary>
    /// Provides extension methods for ControllerBase to generate standardized RFC 7807 problem details responses for
    /// common HTTP error statuses in ASP.NET Core APIs.
    /// </summary>
    /// <remarks>These extension methods simplify returning consistent error responses from API controllers by
    /// encapsulating common HTTP error codes and formatting error details according to the Problem Details for HTTP
    /// APIs specification (RFC 7807). Each method allows specifying a human-readable error detail, an optional error
    /// code, and additional metadata. The resulting response includes standard fields such as status, title, detail,
    /// and instance, as well as custom extensions like code and traceId for enhanced error tracking. These methods are
    /// intended to be used within controller actions to provide clear and structured error information to API
    /// clients.</remarks>
    public static class ControllerBaseExtensions
    {
        /// <summary>
        /// Creates a standardized 400 Bad Request API error response with a specified detail message, error code, and
        /// optional additional data.
        /// </summary>
        /// <remarks>The response conforms to a consistent error format suitable for API clients. Use this
        /// method to provide detailed error information in a standardized way when a request cannot be processed due to
        /// client error.</remarks>
        /// <param name="controller">The controller instance on which this extension method is called. Cannot be null.</param>
        /// <param name="detail">A human-readable explanation specific to this occurrence of the error. Cannot be null.</param>
        /// <param name="code">An application-specific error code to include in the response. Defaults to "bad_request" if not specified.</param>
        /// <param name="extra">An optional object containing additional information to include in the error response. May be null.</param>
        /// <returns>An IActionResult representing a 400 Bad Request response with a structured error payload.</returns>
        public static IActionResult ApiBadRequest(this ControllerBase controller, string detail, string code = "bad_request", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.BadRequest, detail, code, extra);

        /// <summary>
        /// Creates an HTTP 401 Unauthorized API error response with a standardized error format.
        /// </summary>
        /// <remarks>Use this method to return a consistent unauthorized error response from API
        /// controllers. The response includes the provided detail message, error code, and any extra data in a
        /// standardized format suitable for clients.</remarks>
        /// <param name="controller">The controller instance used to generate the response.</param>
        /// <param name="detail">A detailed message describing the reason for the unauthorized error. This value is included in the response
        /// body.</param>
        /// <param name="code">An optional error code to include in the response. Defaults to "unauthorized".</param>
        /// <param name="extra">An optional object containing additional information to include in the error response. May be null.</param>
        /// <returns>An <see cref="IActionResult"/> representing a 401 Unauthorized error response with the specified details.</returns>
        public static IActionResult ApiUnauthorized(this ControllerBase controller, string detail, string code = "unauthorized", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.Unauthorized, detail, code, extra);

        /// <summary>
        /// Creates a 403 Forbidden API response with a standardized error payload.
        /// </summary>
        /// <remarks>Use this method to return a consistent forbidden error response from API actions when
        /// the client does not have permission to access the requested resource.</remarks>
        /// <param name="controller">The controller instance used to generate the response.</param>
        /// <param name="detail">A detailed message describing the reason for the forbidden response. This value is included in the error
        /// payload.</param>
        /// <param name="code">An optional error code to include in the response. Defaults to "forbidden".</param>
        /// <param name="extra">An optional object containing additional information to include in the error payload. May be null.</param>
        /// <returns>An IActionResult representing a 403 Forbidden response with a standardized error body.</returns>
        public static IActionResult ApiForbidden(this ControllerBase controller, string detail, string code = "forbidden", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.Forbidden, detail, code, extra);

        /// <summary>
        /// Creates a standardized 404 Not Found API error response with the specified detail message and optional error
        /// code and additional data.
        /// </summary>
        /// <remarks>Use this method to return a consistent error response when an API resource cannot be
        /// found. The response follows a standard error format, making it suitable for client-side error
        /// handling.</remarks>
        /// <param name="controller">The controller instance used to generate the response.</param>
        /// <param name="detail">A detailed message describing the reason the resource was not found. This value is included in the response
        /// body.</param>
        /// <param name="code">An optional error code to include in the response. Defaults to "not_found" if not specified.</param>
        /// <param name="extra">Optional additional data to include in the error response. Can be null if no extra information is needed.</param>
        /// <returns>An IActionResult representing a 404 Not Found API error response containing the specified detail, error
        /// code, and any additional data.</returns>
        public static IActionResult ApiNotFound(this ControllerBase controller, string detail, string code = "not_found", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.NotFound, detail, code, extra);

        /// <summary>
        /// Creates a 409 Conflict API error response with a standardized error format.
        /// </summary>
        /// <remarks>Use this method to return a consistent error response when a request cannot be
        /// completed due to a conflict with the current state of the resource. The response body includes the provided
        /// detail message, error code, and any additional data supplied.</remarks>
        /// <param name="controller">The controller instance used to generate the response.</param>
        /// <param name="detail">A detailed message describing the reason for the conflict. This information is included in the response
        /// body.</param>
        /// <param name="code">An optional application-specific error code to include in the response. Defaults to "conflict".</param>
        /// <param name="extra">An optional object containing additional data to include in the error response. May be null if no extra
        /// information is needed.</param>
        /// <returns>An IActionResult representing a 409 Conflict response with a standardized error body.</returns>
        public static IActionResult ApiConflict(this ControllerBase controller, string detail, string code = "conflict", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.Conflict, detail, code, extra);

        /// <summary>
        /// Creates an HTTP 422 Unprocessable Entity response with a standardized error payload.
        /// </summary>
        /// <remarks>Use this method to return validation or semantic errors that prevent the server from
        /// processing the request, following the RFC 4918 and RFC 7807 conventions for unprocessable
        /// entities.</remarks>
        /// <param name="controller">The controller instance used to generate the response.</param>
        /// <param name="detail">A human-readable explanation specific to this occurrence of the problem.</param>
        /// <param name="code">An application-specific error code that identifies the error type. Defaults to "unprocessable".</param>
        /// <param name="extra">An optional object containing additional error details to include in the response. May be null.</param>
        /// <returns>An IActionResult representing the 422 Unprocessable Entity response with a standardized error body.</returns>
        public static IActionResult ApiUnprocessable(this ControllerBase controller, string detail, string code = "unprocessable", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.UnprocessableEntity, detail, code, extra);

        /// <summary>
        /// Creates a 429 Too Many Requests API error response with a specified detail message and optional error code
        /// and additional data.
        /// </summary>
        /// <remarks>Use this method to return a standardized error response when a client exceeds allowed
        /// request limits. The response includes the HTTP 429 status code and a JSON body with the provided detail,
        /// code, and any extra data.</remarks>
        /// <param name="controller">The controller instance on which this extension method is called.</param>
        /// <param name="detail">A detailed message describing the reason for the rate limit error. This value is included in the response
        /// body.</param>
        /// <param name="code">An optional error code to include in the response. Defaults to "rate_limited" if not specified.</param>
        /// <param name="extra">An optional object containing additional data to include in the error response. Can be null.</param>
        /// <returns>An IActionResult representing a 429 Too Many Requests error with the specified details.</returns>
        public static IActionResult ApiTooManyRequests(this ControllerBase controller, string detail, string code = "rate_limited", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.TooManyRequests, detail, code, extra);

        /// <summary>
        /// Creates an HTTP 500 Internal Server Error response with a standardized error body for use in API
        /// controllers.
        /// </summary>
        /// <remarks>Use this method to return a consistent error response from API endpoints when an
        /// unexpected server-side error occurs. The response includes the provided detail message and optional extra
        /// data, which can assist clients in diagnosing issues.</remarks>
        /// <param name="controller">The controller instance on which this extension method is called.</param>
        /// <param name="detail">A detailed message describing the internal error. This information is included in the response body.</param>
        /// <param name="code">An optional error code to include in the response. Defaults to "internal" if not specified.</param>
        /// <param name="extra">An optional object containing additional information to include in the error response. May be null.</param>
        /// <returns>An IActionResult representing a 500 Internal Server Error response with a standardized error payload.</returns>
        public static IActionResult ApiInternal(this ControllerBase controller, string detail, string code = "internal", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.InternalServerError, detail, code, extra);

        /// <summary>
        /// Creates a 503 Service Unavailable API error response with a specified detail message and optional error code
        /// and additional data.
        /// </summary>
        /// <remarks>Use this method to indicate that the API or a specific endpoint is temporarily
        /// unavailable. The response follows a standard error format and can include custom error codes and additional
        /// data for clients to interpret the error condition.</remarks>
        /// <param name="controller">The controller instance used to build the API error response.</param>
        /// <param name="detail">A detailed message describing the reason the API is unavailable. This value is included in the response
        /// body.</param>
        /// <param name="code">An optional error code to include in the response. Defaults to "unavailable".</param>
        /// <param name="extra">Optional additional data to include in the error response. Can be null if no extra information is needed.</param>
        /// <returns>An IActionResult representing a 503 Service Unavailable error with the specified details.</returns>
        public static IActionResult ApiUnavailable(this ControllerBase controller, string detail, string code = "unavailable", object? extra = null)
            => controller.BuildApiError(HttpStatusCode.ServiceUnavailable, detail, code, extra);

        private static ObjectResult BuildApiError(this ControllerBase controller, HttpStatusCode status, string detail, string? code, object? extra)
        {
            int statusCode = (int)status;
            var details = new ProblemDetails
            {
                Detail = detail,
                Status = statusCode,
                Instance = controller.HttpContext.Request.Path,
                Title = ReasonPhrases.GetReasonPhrase(statusCode)
            };

            if (!string.IsNullOrWhiteSpace(code))
            {
                details.Extensions["code"] = code;
            }

            AddTraceId(controller, details);
            AddExtra(details, extra);

            return new ObjectResult(details)
            {
                StatusCode = statusCode,
                ContentTypes = { MediaTypeNames.Application.ProblemJson }
            };
        }

        private static void AddTraceId(ControllerBase controller, ProblemDetails details)
        {
            var traceId = controller.HttpContext.TraceIdentifier;
            if (!string.IsNullOrWhiteSpace(traceId))
            {
                details.Extensions["traceId"] = traceId;
            }
        }

        private static void AddExtra(ProblemDetails details, object? extra)
        {
            if (extra is null)
            {
                return;
            }

            if (extra is IReadOnlyDictionary<string, object?> dict)
            {
                foreach (var (key, value) in dict)
                {
                    details.Extensions[key] = value;
                }
                return;
            }

            var props = extra.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                details.Extensions[prop.Name] = prop.GetValue(extra);
            }
        }
    }
}
