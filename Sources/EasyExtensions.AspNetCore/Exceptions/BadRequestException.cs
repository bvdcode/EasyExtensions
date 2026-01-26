// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when a request is unauthorized or invalid for a specified object.
    /// </summary>
    /// <param name="objectName">The name of the object associated with the unauthorized or invalid request.</param>
    public class BadRequestException(string objectName)
        : WebApiException(HttpStatusCode.BadRequest, objectName, "Bad request")
    { }
}