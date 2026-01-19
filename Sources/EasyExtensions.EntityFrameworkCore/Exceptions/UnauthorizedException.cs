// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.EntityFrameworkCore.Exceptions
{
    /// <summary>
    /// Unauthorized exception.
    /// </summary>
    /// <param name="objectName"> Object name. </param>
    [Serializable]
    public class UnauthorizedException(string objectName)
        : WebApiException(HttpStatusCode.Unauthorized, objectName, "Unathorized")
    { }
}
