// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when an attempt is made to create or add an object that already exists.
    /// </summary>
    /// <remarks>This exception corresponds to an HTTP 409 Conflict response and is typically used in web APIs
    /// to indicate that a resource with the specified name already exists. Use this exception to signal duplicate
    /// creation attempts in resource management scenarios.</remarks>
    /// <param name="objectName">The name of the object that caused the conflict.</param>
    public class DuplicateException(string objectName)
        : WebApiException(HttpStatusCode.Conflict, objectName, "Object already exists")
    { }
}
