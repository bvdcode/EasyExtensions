using System.Collections.Generic;
using System.Threading;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a handler for processing streaming requests and returning a sequence of responses asynchronously.
    /// </summary>
    /// <remarks>Implementations of this interface process incoming streaming requests and yield responses as
    /// an asynchronous stream. This is typically used in scenarios where multiple results are produced over time in
    /// response to a single request. The handler should honor cancellation requests via the provided <see
    /// cref="CancellationToken"/>.</remarks>
    /// <typeparam name="TRequest">The type of the streaming request to handle. Must implement <see cref="IStreamRequest{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of the response elements produced by the handler.</typeparam>
    public interface IStreamRequestHandler<in TRequest, out TResponse> where TRequest : IStreamRequest<TResponse>
    {
        /// <summary>
        /// Handles the specified request asynchronously and returns a stream of response messages.
        /// </summary>
        /// <param name="request">The request message to process. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An asynchronous stream of response messages generated as the request is processed.</returns>
        IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
