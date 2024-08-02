using System;
using System.Net;
using System.Diagnostics;
using EasyExtensions.Models;
using System.Collections.Generic;
using EasyExtensions.Abstractions;

namespace EasyExtensions.EntityFrameworkCore.Exceptions
{
    /// <summary>
    /// Base web api exception.
    /// </summary>
    /// <param name="statusCode"> HTTP status code. </param>
    /// <param name="objectName"> Object name. </param>
    /// <param name="message"> Exception message. </param>
    public class WebApiException(HttpStatusCode statusCode, string objectName, string message) : Exception(message), IHttpError
    {
        /// <summary>
        /// HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; } = statusCode;

        /// <summary>
        /// Object name.
        /// </summary>
        public string ObjectName { get; } = objectName;

        /// <summary>
        /// Get error model.
        /// </summary>
        /// <returns> Error model. </returns>
        public ErrorModel GetErrorModel()
        {
            return new()
            {

                Status = (int)StatusCode,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Web API actions errors occurred.",
                TraceId = Activity.Current?.Id ?? "-",
                Errors = new Dictionary<string, string>
                {
                    { ObjectName, Message }
                }
            };
        }
    }
}