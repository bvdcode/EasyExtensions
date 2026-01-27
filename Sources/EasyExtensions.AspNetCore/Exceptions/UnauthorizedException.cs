// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when an operation is attempted without proper authorization.
    /// </summary>
    /// <remarks>This exception corresponds to an HTTP 401 Unauthorized response. It is typically used in web
    /// API scenarios to indicate that the caller must authenticate or does not have permission to access the specified
    /// resource.</remarks>
    /// <param name="objectName">The name of the object or resource for which access was denied.</param>
    /// <param name="message">The error message that describes the reason for the unauthorized access. The default is "Unathorized".</param>
    public class UnauthorizedException(string objectName, string message = "Unathorized")
        : WebApiException(HttpStatusCode.Unauthorized, objectName, message) { }

    /// <summary>
    /// Represents an exception that is thrown when an operation is attempted without the required authorization for a
    /// specific resource type.
    /// </summary>
    /// <typeparam name="T">The type of the resource or entity for which authorization failed.</typeparam>
    /// <param name="message">The error message that explains the reason for the exception. The default is "Unathorized".</param>
    public class UnauthorizedException<T>(string message = "Unathorized")
        : UnauthorizedException(typeof(T).Name, message) { }
}
