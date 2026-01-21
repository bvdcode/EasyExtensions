using EasyExtensions.Mediator.Contracts;
using EasyExtensions.Mediator.NotificationPublishers;
using EasyExtensions.Mediator.Wrappers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator
{
    /// <summary>
    /// Default mediator implementation relying on single- and multi instance delegates for resolving handlers.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INotificationPublisher _publisher;
        private static readonly ConcurrentDictionary<Type, RequestHandlerBase> _requestHandlers = new ConcurrentDictionary<Type, RequestHandlerBase>();
        private static readonly ConcurrentDictionary<Type, NotificationHandlerWrapper> _notificationHandlers = new ConcurrentDictionary<Type, NotificationHandlerWrapper>();
        private static readonly ConcurrentDictionary<Type, StreamRequestHandlerBase> _streamRequestHandlers = new ConcurrentDictionary<Type, StreamRequestHandlerBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider. Can be a scoped or root provider</param>
        public Mediator(IServiceProvider serviceProvider)
            : this(serviceProvider, new ForeachAwaitPublisher()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mediator"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider. Can be a scoped or root provider</param>
        /// <param name="publisher">Notification publisher. Defaults to <see cref="ForeachAwaitPublisher"/>.</param>
        public Mediator(IServiceProvider serviceProvider, INotificationPublisher publisher)
        {
            _serviceProvider = serviceProvider;
            _publisher = publisher;
        }

        /// <summary>
        /// Sends a request to the appropriate handler and returns a task representing the asynchronous operation.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response expected from the request.</typeparam>
        /// <param name="request">The request message to send. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the request operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response from the handler.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a handler for the request type cannot be created.</exception>
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var handler = (RequestHandlerWrapper<TResponse>)_requestHandlers.GetOrAdd(request.GetType(), requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
                var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                return (RequestHandlerBase)wrapper;
            });

            return handler.Handle(request, _serviceProvider, cancellationToken);
        }

        /// <summary>
        /// Sends a request to the appropriate handler for processing asynchronously.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request to send. Must implement the <see cref="IRequest"/> interface.</typeparam>
        /// <param name="request">The request object to be processed. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a handler wrapper for the request type cannot be created.</exception>
        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var handler = (RequestHandlerWrapper)_requestHandlers.GetOrAdd(request.GetType(), requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<>).MakeGenericType(requestType);
                var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
                return (RequestHandlerBase)wrapper;
            });

            return handler.Handle(request, _serviceProvider, cancellationToken);
        }

        /// <summary>
        /// Sends a request to the appropriate handler and returns the response asynchronously.
        /// </summary>
        /// <remarks>The request is dispatched to a handler based on its runtime type. If multiple
        /// handlers are registered for the same request type, the behavior is implementation-specific. The method
        /// supports both requests with and without a response type.</remarks>
        /// <param name="request">The request object to send. Must implement either IRequest or IRequest<TResponse>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the request operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response from the handler,
        /// or null if the request does not produce a response.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the request parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the request object does not implement IRequest or IRequest<TResponse>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a handler wrapper cannot be created for the specified request type.</exception>
        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var handler = _requestHandlers.GetOrAdd(request.GetType(), requestType =>
            {
                Type wrapperType;

                var requestInterfaceType = requestType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
                if (requestInterfaceType is null)
                {
                    requestInterfaceType = requestType.GetInterfaces().FirstOrDefault(i => i == typeof(IRequest));
                    if (requestInterfaceType is null)
                    {
                        throw new ArgumentException($"{requestType.Name} does not implement {nameof(IRequest)}", nameof(request));
                    }

                    wrapperType = typeof(RequestHandlerWrapperImpl<>).MakeGenericType(requestType);
                }
                else
                {
                    var responseType = requestInterfaceType.GetGenericArguments()[0];
                    wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
                }

                var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper for type {requestType}");
                return (RequestHandlerBase)wrapper;
            });

            // call via dynamic dispatch to avoid calling through reflection for performance reasons
            return handler.Handle(request, _serviceProvider, cancellationToken);
        }

        /// <summary>
        /// Publishes a notification to all registered handlers for the specified notification type.
        /// </summary>
        /// <typeparam name="TNotification">The type of notification being published. Must implement <see cref="INotification"/>.</typeparam>
        /// <param name="notification">The notification instance to publish to handlers. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="notification"/> is null.</exception>
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            return PublishNotification(notification, cancellationToken);
        }

        /// <summary>
        /// Publishes a notification to all registered handlers asynchronously.
        /// </summary>
        /// <param name="notification">The notification object to publish. Must implement the INotification interface and cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the notification parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the notification does not implement the INotification interface.</exception>
        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            notification switch
            {
                null => throw new ArgumentNullException(nameof(notification)),
                INotification instance => PublishNotification(instance, cancellationToken),
                _ => throw new ArgumentException($"{nameof(notification)} does not implement ${nameof(INotification)}")
            };

        /// <summary>
        /// Override in a derived class to control how the tasks are awaited. By default the implementation calls the <see cref="INotificationPublisher"/>.
        /// </summary>
        /// <param name="handlerExecutors">Enumerable of tasks representing invoking each notification handler</param>
        /// <param name="notification">The notification being published</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing invoking all handlers</returns>
        protected virtual Task PublishCore(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
            => _publisher.Publish(handlerExecutors, notification, cancellationToken);

        private Task PublishNotification(INotification notification, CancellationToken cancellationToken = default)
        {
            var handler = _notificationHandlers.GetOrAdd(notification.GetType(), notificationType =>
            {
                var wrapperType = typeof(NotificationHandlerWrapperImpl<>).MakeGenericType(notificationType);
                var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper for type {notificationType}");
                return (NotificationHandlerWrapper)wrapper;
            });

            return handler.Handle(notification, _serviceProvider, PublishCore, cancellationToken);
        }

        /// <summary>
        /// Creates an asynchronous stream of responses for the specified stream request.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response elements returned by the stream.</typeparam>
        /// <param name="request">The stream request that defines the parameters and type of responses to be returned. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the stream operation. The default value is <see
        /// cref="CancellationToken.None"/>.</param>
        /// <returns>An asynchronous stream of response elements of type <typeparamref name="TResponse"/> corresponding to the
        /// specified request.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a suitable stream handler cannot be created for the type of <paramref name="request"/>.</exception>
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var streamHandler = (StreamRequestHandlerWrapper<TResponse>)_streamRequestHandlers.GetOrAdd(request.GetType(), requestType =>
            {
                var wrapperType = typeof(StreamRequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
                var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper for type {requestType}");
                return (StreamRequestHandlerBase)wrapper;
            });

            var items = streamHandler.Handle(request, _serviceProvider, cancellationToken);

            return items;
        }

        /// <summary>
        /// Creates an asynchronous stream of response objects for the specified request.
        /// </summary>
        /// <remarks>The returned stream is evaluated asynchronously. Consumers should enumerate the
        /// stream using await foreach. Each call to this method resolves the appropriate handler for the request type
        /// using dependency injection.</remarks>
        /// <param name="request">The request object to process. Must implement the IStreamRequest<TResponse> interface.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the stream operation.</param>
        /// <returns>An asynchronous stream of response objects corresponding to the request. The stream may be empty if there
        /// are no results.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the request parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the request object does not implement the IStreamRequest<TResponse> interface.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a handler wrapper for the request type cannot be created.</exception>
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var handler = _streamRequestHandlers.GetOrAdd(request.GetType(), requestType =>
            {
                var requestInterfaceType = requestType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequest<>))
                    ?? throw new ArgumentException($"{requestType.Name} does not implement IStreamRequest<TResponse>", nameof(request));
                var responseType = requestInterfaceType.GetGenericArguments()[0];
                var wrapperType = typeof(StreamRequestHandlerWrapperImpl<,>).MakeGenericType(requestType, responseType);
                var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper for type {requestType}");
                return (StreamRequestHandlerBase)wrapper;
            });

            return handler.Handle(request, _serviceProvider, cancellationToken);
        }
    }
}