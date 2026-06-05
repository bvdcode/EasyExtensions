// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when an operation on a file cannot be completed because one or more
    /// preconditions specified for the request were not met.
    /// </summary>
    /// <remarks>This exception corresponds to an HTTP 412 Precondition Failed response. It is typically used in
    /// web API scenarios to indicate that a conditional request (for example, one guarded by an ETag, a version, or a
    /// last-modified timestamp) could not be applied to the target file because its current state does not satisfy the
    /// supplied precondition.</remarks>
    /// <param name="objectName">The name of the file or entity for which the precondition failed. This value is used to provide context
    /// for the error.</param>
    /// <param name="message">The error message that describes the reason for the exception. The default is "File precondition failed".</param>
    /// <param name="extra">Optional additional error details. This parameter can be used to provide extra information about the error, such as the
    /// expected and actual ETag, version, or any other relevant context that may help in understanding why the precondition was not met.</param>
    public class FilePreconditionFailedException(string objectName, string message = "File precondition failed", object? extra = null)
        : WebApiException(HttpStatusCode.PreconditionFailed, objectName, message, extra)
    { }

    /// <summary>
    /// Represents an exception that is thrown when a precondition on a file of type T was not met,
    /// typically corresponding to an HTTP 412 Precondition Failed response.
    /// </summary>
    /// <typeparam name="T">The type of the file or entity for which the precondition failed.</typeparam>
    /// <param name="message">The error message that describes the reason for the exception. The default is "File precondition failed".</param>
    /// <param name="extra">Optional additional error details. This parameter can be used to provide extra information about the error, such as the
    /// expected and actual ETag, version, or any other relevant context that may help in understanding why the precondition was not met.</param>
    public class FilePreconditionFailedException<T>(string message = "File precondition failed", object? extra = null)
        : FilePreconditionFailedException(typeof(T).Name, message, extra)
    { }
}
