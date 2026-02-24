// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown to indicate a bad request (HTTP 400) in a web API operation.
    /// </summary>
    /// <param name="objectName">The name of the object or entity associated with the bad request. This value is used to provide context for the
    /// error.</param>
    /// <param name="message">The error message that describes the reason for the bad request. The default is "Bad request".</param>
    public class BadRequestException(string objectName, string message = "Bad request")
        : WebApiException(HttpStatusCode.BadRequest, objectName, message)
    { }

    /// <summary>
    /// Represents an exception that is thrown to indicate a bad request error associated with a specific resource type.
    /// </summary>
    /// <typeparam name="T">The type of the resource or entity related to the bad request.</typeparam>
    /// <param name="message">The error message that describes the reason for the bad request. The default is "Bad request".</param>
    public class BadRequestException<T>(string message = "Bad request")
        : BadRequestException(typeof(T).Name, message)
    { }
}