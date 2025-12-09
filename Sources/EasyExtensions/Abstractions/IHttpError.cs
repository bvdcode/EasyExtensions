using EasyExtensions.Models;
using System.Net;

namespace EasyExtensions.Abstractions
{
    /// <summary>
    /// Interface for HTTP error.
    /// </summary>
    public interface IHttpError
    {
        /// <summary>
        /// Gets error model.
        /// </summary>
        /// <returns> Error model. </returns>
        ErrorModel GetErrorModel();

        /// <summary>
        /// Gets status code.
        /// </summary>
        HttpStatusCode StatusCode { get; }
    }
}