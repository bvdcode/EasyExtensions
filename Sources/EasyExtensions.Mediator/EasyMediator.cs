using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using EasyExtensions.Mediator.Contracts;

namespace EasyExtensions.Mediator
{
    /// <summary>
    /// Provides a mediator implementation that coordinates the sending of requests, publishing of notifications, and
    /// streaming of responses using registered handlers and processors.
    /// </summary>
    /// <remarks>
    /// EasyMediator enables decoupled communication between components by dispatching requests,
    /// notifications, and stream requests to their corresponding handlers. It supports both strongly-typed and
    /// object-based APIs, allowing for flexible integration scenarios. Handlers, pre-processors, post-processors, and
    /// exception handlers are resolved from the provided service provider. This class is typically used in applications
    /// that follow the mediator pattern to centralize request and notification handling. Thread safety depends on the
    /// underlying service provider and registered handlers.
    /// </remarks>
    public class EasyMediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the EasyMediator class using the specified service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies for handling requests and notifications.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="serviceProvider"/> parameter is null.</exception>
        public EasyMediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Creates an asynchronous stream of response objects for the specified stream request.
        /// </summary>
        /// <remarks>
        /// The returned stream yields objects of the response type specified by the request's
        /// <see cref="IStreamRequest{TResponse}"/> implementation, boxed as <see cref="object"/>. Consumers should cast each element to the
        /// expected response type.
        /// </remarks>
        /// <param name="request">The request object that implements the <see cref="IStreamRequest{TResponse}"/> interface. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the stream operation.</param>
        /// <returns>
        /// An asynchronous stream of response objects corresponding to the request. Each element in the stream is of
        /// the response type defined by the request.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the request object does not implement <see cref="IStreamRequest{TResponse}"/>.</exception>
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();
            var streamInterface = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IStreamRequest<>))
                ?? throw new InvalidOperationException($"Request type '{requestType.FullName}' does not implement IStreamRequest<TResponse>.");
            var responseType = streamInterface.GetGenericArguments()[0];
            var method = typeof(EasyMediator)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => m.Name == nameof(CreateStream) && m.IsGenericMethodDefinition && m.GetParameters().Length == 2);

            var generic = method.MakeGenericMethod(responseType);
            // Invoke returns IAsyncEnumerable<T>, which we box to object and cast back to IAsyncEnumerable<object?> by adapting
            var result = generic.Invoke(this, new object[] { request, cancellationToken });

            // We need to adapt IAsyncEnumerable<T> to IAsyncEnumerable<object?>
            return AdaptAsyncEnumerable(result, responseType);
        }

        /// <summary>
        /// Creates an asynchronous stream of responses for the specified stream request.
        /// </summary>
        /// <remarks>
        /// The returned stream is produced by the handler registered for the specific request
        /// type. Ensure that a compatible handler is registered in the service provider before calling this
        /// method.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the response elements produced by the stream.</typeparam>
        /// <param name="request">The stream request to process. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the stream operation. The default value is <see
        /// cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// An asynchronous stream of response elements of type <typeparamref name="TResponse"/> generated by the
        /// registered handler for the request.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no handler is registered for the request type or if the handler does not implement a <c>Handle</c>
        /// method.</exception>
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();
            var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType) ?? throw new InvalidOperationException($"No handler registered for {handlerType.FullName}");
            var handleMethod = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handler {handlerType.FullName} does not have a Handle method.");
            var stream = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
            return (IAsyncEnumerable<TResponse>)stream!;
        }

        /// <summary>
        /// Publishes a notification to all registered handlers for the notification's type.
        /// </summary>
        /// <param name="notification">The notification object to publish. Must implement the <see cref="INotification"/> interface.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="notification"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the notification does not implement the <see cref="INotification"/> interface.</exception>
        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var notificationType = notification.GetType();
            if (!typeof(INotification).IsAssignableFrom(notificationType))
            {
                throw new InvalidOperationException($"Notification type '{notificationType.FullName}' does not implement INotification.");
            }

            var method = typeof(EasyMediator)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .First(m => m.Name == nameof(Publish) && m.IsGenericMethodDefinition);

            var generic = method.MakeGenericMethod(notificationType);
            var task = (Task)generic.Invoke(this, new object[] { notification, cancellationToken })!;
            return task;
        }

        /// <summary>
        /// Publishes a notification to all registered handlers for the specified notification type.
        /// </summary>
        /// <remarks>
        /// All handlers for the specified notification type are invoked asynchronously. The
        /// order in which handlers are invoked is not guaranteed.
        /// </remarks>
        /// <typeparam name="TNotification">The type of notification being published. Must implement the <see cref="INotification"/> interface.</typeparam>
        /// <param name="notification">The notification instance to publish to handlers. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="notification"/> is null.</exception>
        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var handlers = GetEnumerable(typeof(INotificationHandler<>).MakeGenericType(typeof(TNotification)));
            foreach (var handler in handlers)
            {
                var handleMethod = handler.GetType().GetMethod("Handle");
                if (handleMethod == null) continue;
                var taskObj = handleMethod.Invoke(handler, new object[] { notification!, cancellationToken });
                if (taskObj is Task task)
                {
                    await task.ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Sends a request to the appropriate handler and returns the response asynchronously.
        /// </summary>
        /// <remarks>
        /// The request is processed by any registered pre-processors and post-processors. If no
        /// handler is registered for the request type, an <see cref="InvalidOperationException"/> is thrown. Exceptions thrown during
        /// processing may be handled by registered exception handlers or actions.
        /// </remarks>
        /// <typeparam name="TResponse">The type of the response expected from the request.</typeparam>
        /// <param name="request">The request to send. Must implement the <see cref="IRequest{TResponse}"/> interface.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response from the handler.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> parameter is null.</exception>
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();

            // Pre-processors
            await InvokePreProcessorsAsync(requestType, request, cancellationToken).ConfigureAwait(false);

            TResponse response;
            try
            {
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
                var handler = _serviceProvider.GetService(handlerType) ?? throw new InvalidOperationException($"No handler registered for {handlerType.FullName}");
                var handleMethod = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handler {handlerType.FullName} does not have a Handle method.");
                var taskObj = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
                response = await AwaitTaskWithResult<TResponse>(taskObj).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var handled = await TryHandleRequestExceptionAsync(requestType, request!, typeof(TResponse), ex, cancellationToken).ConfigureAwait(false);
                if (handled.handled)
                {
                    return (TResponse)handled.response!;
                }

                // Exception actions (side effects), then rethrow
                await InvokeExceptionActionsAsync(requestType, request!, ex, cancellationToken).ConfigureAwait(false);
                throw;
            }

            // Post-processors
            await InvokePostProcessorsAsync(requestType, request, response, cancellationToken).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Sends a request to the appropriate handler for processing, invoking any registered pre-processors and
        /// exception actions as needed.
        /// </summary>
        /// <remarks>
        /// If no handler is registered for the request type, an <see cref="InvalidOperationException"/> is thrown. Pre-processors are invoked before the handler, and exception
        /// actions are executed if an exception occurs during processing.
        /// </remarks>
        /// <typeparam name="TRequest">The type of the request to send. Must implement the <see cref="IRequest"/> interface.</typeparam>
        /// <param name="request">The request object to be processed. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();

            // Pre-processors
            await InvokePreProcessorsAsync(requestType, request, cancellationToken).ConfigureAwait(false);

            try
            {
                var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
                var handler = _serviceProvider.GetService(handlerType) ?? throw new InvalidOperationException($"No handler registered for {handlerType.FullName}");
                var handleMethod = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handler {handlerType.FullName} does not have a Handle method.");
                var taskObj = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
                if (taskObj is Task task)
                {
                    await task.ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // Exception actions only for non-response requests
                await InvokeExceptionActionsAsync(requestType, request!, ex, cancellationToken).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>
        /// Sends a request to the appropriate handler and asynchronously returns the response, if any.
        /// </summary>
        /// <remarks>
        /// The type of the request determines whether a response is expected. If the request
        /// implements <see cref="IRequest{TResponse}"/>, the method returns the response from the handler. If the request implements
        /// <see cref="IRequest"/> (without a response type), the method returns <see langword="null"/> after the handler completes. This method is
        /// typically used in scenarios where the request type is not known at compile time.
        /// </remarks>
        /// <param name="request">The request object to send. Must implement either <see cref="IRequest"/> or <see cref="IRequest{TResponse}"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
        /// <returns>
        /// An object representing the response from the handler if the request implements <see cref="IRequest{TResponse}"/>;
        /// otherwise, <see langword="null"/> if the request implements <see cref="IRequest"/> with no response.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="request"/> parameter is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the request type does not implement <see cref="IRequest"/> or <see cref="IRequest{TResponse}"/>.</exception>
        public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();
            var irequestWithResponse = requestType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));

            if (irequestWithResponse != null)
            {
                var responseType = irequestWithResponse.GetGenericArguments()[0];
                var method = typeof(EasyMediator)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .First(m => m.Name == nameof(Send) && m.IsGenericMethodDefinition && m.GetParameters().First().ParameterType.IsGenericType);
                var generic = method.MakeGenericMethod(responseType);
                var taskObj = generic.Invoke(this, new object[] { request, cancellationToken });
                var result = await AwaitTaskWithResult<object?>(taskObj).ConfigureAwait(false);
                return result;
            }

            if (typeof(IRequest).IsAssignableFrom(requestType))
            {
                var method = typeof(EasyMediator)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .First(m => m.Name == nameof(Send) && m.IsGenericMethodDefinition && m.GetParameters().First().ParameterType.IsGenericType == false && m.GetParameters().Length == 2);
                var generic = method.MakeGenericMethod(requestType);
                var task = (Task)generic.Invoke(this, new object[] { request, cancellationToken })!;
                await task.ConfigureAwait(false);
                return null;
            }

            throw new InvalidOperationException($"Request type '{requestType.FullName}' does not implement IRequest or IRequest<TResponse>.");
        }

        private IEnumerable<object> GetEnumerable(Type serviceType)
        {
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            var instance = _serviceProvider.GetService(enumerableType);
            return instance as IEnumerable<object> ?? Enumerable.Empty<object>();
        }

        private static async Task<T> AwaitTaskWithResult<T>(object? taskObj)
        {
            if (taskObj == null)
            {
                throw new InvalidOperationException("Handler returned null task.");
            }
            if (taskObj is Task task)
            {
                await task.ConfigureAwait(false);
                var taskType = taskObj.GetType();
                var resultProperty = taskType.GetProperty("Result");
                if (resultProperty != null)
                {
                    return (T)resultProperty.GetValue(taskObj)!;
                }
                return default!;
            }
            throw new InvalidOperationException("Object is not a Task.");
        }

        private static IAsyncEnumerable<object?> AdaptAsyncEnumerable(object? asyncEnumerableObj, Type itemType)
        {
            if (asyncEnumerableObj == null)
            {
                return EmptyAsyncEnumerable();
            }

            var method = typeof(EasyMediator).GetMethod(nameof(AdaptAsyncEnumerableGeneric), BindingFlags.NonPublic | BindingFlags.Static);
            var generic = method!.MakeGenericMethod(itemType);
            return (IAsyncEnumerable<object?>)generic.Invoke(null, new[] { asyncEnumerableObj })!;
        }

        private static async IAsyncEnumerable<object?> AdaptAsyncEnumerableGeneric<T>(IAsyncEnumerable<T> source)
        {
            await foreach (var item in source)
            {
                yield return item;
            }
        }

        private async Task InvokePreProcessorsAsync(Type requestType, object request, CancellationToken cancellationToken)
        {
            var preProcessorType = typeof(IRequestPreProcessor<>).MakeGenericType(requestType);
            var preProcessors = GetEnumerable(preProcessorType);
            foreach (var pre in preProcessors)
            {
                var processMethod = pre.GetType().GetMethod("Process");
                if (processMethod == null) continue;
                var taskObj = processMethod.Invoke(pre, new object[] { request, cancellationToken });
                if (taskObj is Task task)
                {
                    await task.ConfigureAwait(false);
                }
            }
        }

        private async Task InvokePostProcessorsAsync<TResponse>(Type requestType, object request, TResponse response, CancellationToken cancellationToken)
        {
            var postProcessorType = typeof(IRequestPostProcessor<,>).MakeGenericType(requestType, typeof(TResponse));
            var postProcessors = GetEnumerable(postProcessorType);
            foreach (var post in postProcessors)
            {
                var processMethod = post.GetType().GetMethod("Process");
                if (processMethod == null) continue;
                var taskObj = processMethod.Invoke(post, new object[] { request, response!, cancellationToken });
                if (taskObj is Task task)
                {
                    await task.ConfigureAwait(false);
                }
            }
        }

        private async Task<(bool handled, object? response)> TryHandleRequestExceptionAsync(Type requestType, object request, Type responseType, Exception exception, CancellationToken cancellationToken)
        {
            // If response is a value type, there can't be a handler because of interface constraint where TResponse : class
            if (responseType.IsValueType)
            {
                return (false, null);
            }

            var exType = exception.GetType();
            var handlerInterface = typeof(IRequestExceptionHandler<,,>).MakeGenericType(requestType, responseType, exType);
            var handlers = GetEnumerable(handlerInterface);
            foreach (var handler in handlers)
            {
                var stateType = typeof(RequestExceptionHandlerState<>).MakeGenericType(responseType);
                var state = Activator.CreateInstance(stateType);
                var handleMethod = handlerInterface.GetMethod("Handle");
                if (handleMethod == null) continue;
                var taskObj = handleMethod.Invoke(handler, new object[] { request, exception, state!, cancellationToken });
                if (taskObj is Task task)
                {
                    await task.ConfigureAwait(false);
                }

                var handledProp = stateType.GetProperty("Handled");
                var handled = (bool)(handledProp!.GetValue(state) ?? false);
                if (handled)
                {
                    var responseProp = stateType.GetProperty("Response");
                    var response = responseProp!.GetValue(state);
                    return (true, response);
                }
            }
            return (false, null);
        }

        private async Task InvokeExceptionActionsAsync(Type requestType, object request, Exception exception, CancellationToken cancellationToken)
        {
            var exType = exception.GetType();
            var actionInterface = typeof(IRequestExceptionAction<,>).MakeGenericType(requestType, exType);
            var actions = GetEnumerable(actionInterface);
            foreach (var action in actions)
            {
                var executeMethod = action.GetType().GetMethod("Execute");
                if (executeMethod == null) continue;
                var taskObj = executeMethod.Invoke(action, new object[] { request, exception, cancellationToken });
                if (taskObj is Task task)
                {
                    await task.ConfigureAwait(false);
                }
            }
        }

        private static async IAsyncEnumerable<object?> EmptyAsyncEnumerable()
        {
            await Task.CompletedTask;
            yield break;
        }
    }
}
