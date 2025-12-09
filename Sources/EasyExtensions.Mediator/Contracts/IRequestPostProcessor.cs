using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a post-processor for requests, allowing additional actions to be performed after a request handler has
    /// processed a request and produced a response.
    /// </summary>
    /// <remarks>Implement this interface to execute logic that should run after the main request handler
    /// completes, such as logging, auditing, or modifying the response. Post-processors are invoked after the request
    /// handler and can observe both the request and its response.</remarks>
    /// <typeparam name="TRequest">The type of the request message. Must not be null.</typeparam>
    /// <typeparam name="TResponse">The type of the response message produced by the request handler.</typeparam>
    public interface IRequestPostProcessor<in TRequest, in TResponse> where TRequest : notnull
    {
        /// <summary>
        /// Processes the specified request and response asynchronously.
        /// </summary>
        /// <param name="request">The request object containing the data to be processed. Cannot be null.</param>
        /// <param name="response">The response object to be populated or modified during processing. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>A task that represents the asynchronous processing operation.</returns>
        Task Process(TRequest request, TResponse response, CancellationToken cancellationToken = default);
    }
}
