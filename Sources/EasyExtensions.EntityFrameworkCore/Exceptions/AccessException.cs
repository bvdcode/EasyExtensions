using System;
using System.Net;

namespace EasyExtensions.EntityFrameworkCore.Exceptions
{
    /// <summary>
    /// Duplicate exception.
    /// </summary>
    /// <param name="objectName"> Object name. </param>
    [Serializable]
    public class AccessException(string objectName)
        : WebApiException(HttpStatusCode.Forbidden, objectName, "Access denied") { }
}