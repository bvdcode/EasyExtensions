// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Net;

namespace EasyExtensions.EntityFrameworkCore.Exceptions
{
    /// <summary>
    /// Entity not found exception.
    /// </summary>
    /// <param name="objectName"> Object name. </param>
    [Serializable]
    public class EntityNotFoundException(string objectName)
        : WebApiException(HttpStatusCode.NotFound, objectName, "Entity was not found")
    { }
}
