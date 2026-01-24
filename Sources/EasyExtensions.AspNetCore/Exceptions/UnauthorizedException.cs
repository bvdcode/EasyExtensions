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
    public class UnauthorizedException(string objectName)
        : WebApiException(HttpStatusCode.Unauthorized, objectName, "Unathorized")
    { }
}
