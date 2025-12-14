using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a contract for sending requests and receiving responses or streams in a mediator pattern.
    /// </summary>
    /// <remarks>The ISender interface provides methods for sending requests and receiving either a single
    /// response or a stream of responses. It supports both strongly-typed and loosely-typed request objects, as well as
    /// asynchronous streaming scenarios. Implementations are typically used to decouple request handling from the
    /// sender, enabling flexible and testable application architectures.</remarks>
    public interface ISender
    {
        /// <summary>
        /// Sends the specified request and asynchronously returns a response of the specified type.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response expected from the request.</typeparam>
        /// <param name="request">The request to send. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous send operation. The task result contains the response to the
        /// request.</returns>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a request for processing without expecting a response.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to send. Must implement the IRequest interface.</typeparam>
        /// <param name="request">The request object to be sent for processing. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;

        /// <summary>
        /// Sends the specified request asynchronously and returns the response object.
        /// </summary>
        /// <param name="request">The request object to be sent. Cannot be null. The type and structure of the request must be supported by
        /// the implementation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
        /// <returns>A task that represents the asynchronous send operation. The task result contains the response object, or
        /// null if no response is returned.</returns>
        Task<object?> Send(object request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an asynchronous stream of response objects based on the specified request.
        /// </summary>
        /// <param name="request">An object representing the request parameters used to generate the stream. The structure and type of this
        /// object must be compatible with the implementation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous stream operation.</param>
        /// <returns>An asynchronous sequence of response objects. Each element in the sequence represents a part of the response
        /// generated for the specified request.</returns>
        IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an asynchronous stream of responses for the specified streaming request.
        /// </summary>
        /// <remarks>The returned stream supports asynchronous iteration and may yield zero or more
        /// responses depending on the request and server behavior. The operation can be cancelled by passing a
        /// cancellation token.</remarks>
        /// <typeparam name="TResponse">The type of the response elements returned by the stream.</typeparam>
        /// <param name="request">The streaming request that defines the parameters and context for the operation. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the streaming operation. The default value is None.</param>
        /// <returns>An asynchronous stream that yields response elements of type TResponse as they become available.</returns>
        IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
