// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Represents an exception that is thrown when an operation cannot be completed because the storage quota
    /// associated with an object has been exceeded.
    /// </summary>
    /// <remarks>This exception corresponds to an HTTP 507 Insufficient Storage response. It is typically used
    /// in web API scenarios to indicate that the server is unable to store the representation needed to complete the
    /// request because the configured storage limit for the resource has been reached.</remarks>
    /// <param name="objectName">The name of the object or entity for which the storage quota was exceeded. This value is used to provide context
    /// for the error.</param>
    /// <param name="message">The error message that describes the reason for the exception. The default is "Storage quota exceeded".</param>
    /// <param name="extra">Optional additional error details. This parameter can be used to provide extra information about the error, such as the
    /// current usage, the configured quota, or any other relevant context that may help in understanding the limit being reached.</param>
    public class StorageQuotaExceededException(string objectName, string message = "Storage quota exceeded", object? extra = null)
        : WebApiException(HttpStatusCode.InsufficientStorage, objectName, message, extra)
    { }

    /// <summary>
    /// Represents an exception that is thrown when the storage quota associated with a resource of type T is exceeded,
    /// typically corresponding to an HTTP 507 Insufficient Storage response.
    /// </summary>
    /// <typeparam name="T">The type of the resource or entity for which the storage quota was exceeded.</typeparam>
    /// <param name="message">The error message that describes the reason for the exception. The default is "Storage quota exceeded".</param>
    /// <param name="extra">Optional additional error details. This parameter can be used to provide extra information about the error, such as the
    /// current usage, the configured quota, or any other relevant context that may help in understanding the limit being reached.</param>
    public class StorageQuotaExceededException<T>(string message = "Storage quota exceeded", object? extra = null)
        : StorageQuotaExceededException(typeof(T).Name, message, extra)
    { }
}
