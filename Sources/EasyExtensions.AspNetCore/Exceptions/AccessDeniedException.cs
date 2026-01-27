// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when access to a specified object is denied due to insufficient
    /// permissions.
    /// </summary>
    /// <param name="objectName">The name of the object for which access was denied. This value is used to identify the resource involved in the
    /// access violation.</param>
    /// <param name="message">The error message that explains the reason for the exception. If not specified, a default message of "Access
    /// denied" is used.</param>
    public class AccessDeniedException(string objectName, string message = "Access denied")
        : WebApiException(HttpStatusCode.Forbidden, objectName, message) { }

    /// <summary>
    /// Represents an exception that is thrown when access to a resource of type T is denied, typically corresponding to
    /// an HTTP 403 Forbidden response.
    /// </summary>
    /// <typeparam name="T">The type of the resource for which access was denied.</typeparam>
    /// <param name="message">The error message that describes the reason for the access denial.</param>
    public class AccessDeniedException<T>(string message = "Access denied")
        : AccessDeniedException(typeof(T).Name, message) { }
}
