// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a requested entity cannot be found.
    /// </summary>
    /// <param name="objectName">The name of the entity that was not found. This value is included in the exception details to identify the
    /// missing entity.</param>
    /// <param name="message">The error message that describes the reason for the exception. The default is "Entity was not found".</param>
    public class EntityNotFoundException(string objectName, string message = "Entity was not found")
        : WebApiException(HttpStatusCode.NotFound, objectName, message)
    { }

    /// <summary>
    /// Represents an exception that is thrown when an entity of the specified type cannot be found.
    /// </summary>
    /// <typeparam name="T">The type of the entity that was not found.</typeparam>
    /// <param name="message">The error message that explains the reason for the exception. If not specified, a default message is used.</param>
    public class EntityNotFoundException<T>(string message = "Entity was not found")
        : EntityNotFoundException(typeof(T).Name, message)
    { }
}
