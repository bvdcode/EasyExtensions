using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a contract for handling exceptions that occur during the processing of a request and determining how the
    /// request pipeline should proceed.
    /// </summary>
    /// <remarks>Implementations of this interface can be used to customize exception handling logic within a
    /// request processing pipeline. The handler can inspect the exception and request, and decide whether to mark the
    /// exception as handled and optionally provide a response. This enables scenarios such as logging, transforming
    /// exceptions into responses, or allowing the pipeline to continue after certain exceptions.</remarks>
    /// <typeparam name="TRequest">The type of the request message being processed. Must not be null.</typeparam>
    /// <typeparam name="TResponse">The type of the response message that may be produced by the handler.</typeparam>
    /// <typeparam name="TException">The type of exception to handle. Must derive from Exception.</typeparam>
    public interface IRequestExceptionHandler<in TRequest, TResponse, in TException> where TRequest : notnull where TException : Exception where TResponse : class
    {
        /// <summary>
        /// Handles an exception that occurred during the processing of a request and determines how the request
        /// pipeline should proceed.
        /// </summary>
        /// <param name="request">The request message that was being processed when the exception was thrown.</param>
        /// <param name="exception">The exception instance that was caught during request processing.</param>
        /// <param name="state">The handler state object used to indicate whether the exception has been handled and to provide a response
        /// if appropriate.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the exception handling operation.</param>
        /// <returns>A task that represents the asynchronous exception handling operation.</returns>
        Task Handle(TRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken = default);
    }
}
