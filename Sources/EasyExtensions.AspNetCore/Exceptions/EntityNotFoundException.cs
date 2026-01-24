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
    public class EntityNotFoundException(string objectName)
        : WebApiException(HttpStatusCode.NotFound, objectName, "Entity was not found")
    { }
}
