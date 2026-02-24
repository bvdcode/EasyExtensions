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
    /// <param name="message">The error message that describes the reason for the conflict. The default is "Object already exists."</param>
    public class DuplicateException(string objectName, string message = "Object already exists")
        : WebApiException(HttpStatusCode.Conflict, objectName, message)
    { }

    /// <summary>
    /// Represents an exception that is thrown when an attempt is made to create or add a duplicate object of the
    /// specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object that caused the duplication error.</typeparam>
    /// <param name="message">The error message that explains the reason for the exception. The default is "Object already exists."</param>
    public class DuplicateException<T>(string message = "Object already exists")
        : DuplicateException(typeof(T).Name, message)
    { }
}
