using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a handler for processing a request and returning a response asynchronously.
    /// </summary>
    /// <remarks>Implement this interface to define custom logic for handling specific request types in a
    /// request/response pattern. The handler is typically invoked by a mediator or dispatcher component.
    /// Implementations should be thread-safe if they are shared across multiple requests.</remarks>
    /// <typeparam name="TRequest">The type of the request to handle. Must implement <see cref="IRequest{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles the specified request and returns a response asynchronously.
        /// </summary>
        /// <param name="request">The request message to process. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response to the request.</returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a handler for processing a request of a specified type asynchronously.
    /// </summary>
    /// <remarks>Implement this interface to define custom logic for handling requests of type <typeparamref
    /// name="TRequest"/>. The handler is typically invoked by a mediator or dispatcher component. Implementations
    /// should ensure thread safety if the handler will be used concurrently.</remarks>
    /// <typeparam name="TRequest">The type of request to handle. Must implement the <see cref="IRequest"/> interface.</typeparam>
    public interface IRequestHandler<in TRequest> where TRequest : IRequest
    {
        /// <summary>
        /// Handles the specified request asynchronously.
        /// </summary>
        /// <param name="request">The request message to be processed. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        Task Handle(TRequest request, CancellationToken cancellationToken = default);
    }
}
