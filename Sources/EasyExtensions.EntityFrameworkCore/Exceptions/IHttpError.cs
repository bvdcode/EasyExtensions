using System.Net;

namespace EasyExtensions.EntityFrameworkCore.Exceptions
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