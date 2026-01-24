// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.AspNetCore.Exceptions
{
    /// <summary>
    /// Access exception.
    /// </summary>
    /// <param name="objectName"> Object name. </param>
    public class AccessException(string objectName)
        : WebApiException(HttpStatusCode.Forbidden, objectName, "Access denied")
    { }
}
