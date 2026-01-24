using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyExtensions.Mediator.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.Mediator.Wrappers
{
    /// <summary>
    /// Provides a base class for handling requests in an asynchronous manner.
    /// </summary>
    /// <remarks>Implement this class to define custom request handling logic. Derived classes should override
    /// the Handle method to process incoming requests using the provided service provider and cancellation
    /// token.</remarks>
    public abstract class RequestHandlerBase
    {
        /// <summary>
        /// Handles the specified request asynchronously using the provided service provider and cancellation token.
        /// </summary>
        /// <param name="request">The request object to be processed. The type and structure of this object determine how the request is
        /// handled.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required to process the request. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response object, or null if
        /// no response is produced.</returns>
        public abstract Task<object?> Handle(object request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides an abstract base class for handling requests that return a response of the specified type.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response returned by the request handler.</typeparam>
    public abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
    {
        /// <summary>
        /// Handles the specified request asynchronously using the provided service provider and cancellation token.
        /// </summary>
        /// <param name="request">The request message to be processed. Cannot be null.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required to handle the request. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response to the request.</returns>
        public abstract Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides an abstract base class for handling requests using a specified service provider and cancellation token.
    /// </summary>
    /// <remarks>This class is intended to be inherited by types that implement request handling logic. It
    /// defines a contract for processing requests in a manner compatible with dependency injection and cancellation
    /// patterns commonly used in .NET applications.</remarks>
    public abstract class RequestHandlerWrapper : RequestHandlerBase
    {
        /// <summary>
        /// Handles the specified request asynchronously using the provided service provider and cancellation token.
        /// </summary>
        /// <param name="request">The request to be processed. Must not be null.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required to handle the request. Must not be null.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Unit"/> value upon
        /// completion.</returns>
        public abstract Task<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides an implementation of a request handler wrapper for handling requests and responses using the specified
    /// request and response types.
    /// </summary>
    /// <remarks>This class enables the execution of request handlers and associated pipeline behaviors for a
    /// given request and response type. It is typically used internally by the Mediator framework to facilitate dynamic
    /// invocation of request handlers and pipeline behaviors. Thread safety and lifetime management depend on the
    /// underlying service provider and registered handlers.</remarks>
    /// <typeparam name="TRequest">The type of the request message. Must implement <see cref="IRequest{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
    public class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles the specified request using the provided service provider and cancellation token.
        /// </summary>
        /// <param name="request">The request object to be handled. Must implement the <see cref="IRequest{TResponse}"/> interface.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required to process the request.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by
        /// handling the request, or <see langword="null"/> if no response is produced.</returns>
        public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken) =>
            await Handle((IRequest<TResponse>)request, serviceProvider, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Handles the specified request by invoking the appropriate request handler and applying all configured
        /// pipeline behaviors in order.
        /// </summary>
        /// <remarks>Pipeline behaviors are executed in reverse registration order, wrapping the request
        /// handler. This method resolves all required services from the provided <paramref
        /// name="serviceProvider"/>.</remarks>
        /// <param name="request">The request message to be handled. Must implement <see cref="IRequest{TResponse}"/>.</param>
        /// <param name="serviceProvider">The service provider used to resolve the request handler and pipeline behaviors.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by the
        /// request handler after all pipeline behaviors have been applied.</returns>
        public override Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            Task<TResponse> Handler(CancellationToken t = default) => serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>()
                .Handle((TRequest)request, t == default ? cancellationToken : t);

            return serviceProvider
                .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .Aggregate((RequestHandlerDelegate<TResponse>)Handler,
                    (next, pipeline) => (t) => pipeline.Handle((TRequest)request, next, t == default ? cancellationToken : t))();
        }
    }

    /// <summary>
    /// Provides a wrapper implementation for handling requests of a specific type using the Mediator pipeline and
    /// service provider.
    /// </summary>
    /// <remarks>This class is typically used internally by the Mediator framework to invoke request handlers
    /// and pipeline behaviors for a given request type. It resolves the appropriate <see
    /// cref="IRequestHandler{TRequest}"/> and any registered <see cref="IPipelineBehavior{TRequest, Unit}"/> instances
    /// from the provided <see cref="IServiceProvider"/>.</remarks>
    /// <typeparam name="TRequest">The type of request to handle. Must implement <see cref="IRequest"/>.</typeparam>
    public class RequestHandlerWrapperImpl<TRequest> : RequestHandlerWrapper
        where TRequest : IRequest
    {
        /// <summary>
        /// Handles the specified request using the provided service provider and cancellation token.
        /// </summary>
        /// <param name="request">The request object to be handled. Must implement the IRequest interface.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required to handle the request.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response object, or null if
        /// no response is produced.</returns>
        public override async Task<object?> Handle(object request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken) =>
            await Handle((IRequest)request, serviceProvider, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Handles the specified request by invoking the appropriate request handler and applying all configured
        /// pipeline behaviors asynchronously.
        /// </summary>
        /// <remarks>Pipeline behaviors are executed in reverse registration order, wrapping the request
        /// handler. All dependencies are resolved from the provided <paramref name="serviceProvider"/>. The request is
        /// cast to <typeparamref name="TRequest"/> before handling.</remarks>
        /// <param name="request">The request message to be handled. Must be of type <typeparamref name="TRequest"/>.</param>
        /// <param name="serviceProvider">The service provider used to resolve the request handler and pipeline behaviors.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Unit"/> value when
        /// the request has been handled.</returns>
        public override Task<Unit> Handle(IRequest request, IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            async Task<Unit> Handler(CancellationToken t = default)
            {
                await serviceProvider.GetRequiredService<IRequestHandler<TRequest>>()
                    .Handle((TRequest)request, t == default ? cancellationToken : t);

                return Unit.Value;
            }

            return serviceProvider
                .GetServices<IPipelineBehavior<TRequest, Unit>>()
                .Reverse()
                .Aggregate((RequestHandlerDelegate<Unit>)Handler,
                    (next, pipeline) => (t) => pipeline.Handle((TRequest)request, next, t == default ? cancellationToken : t))();
        }
    }
}