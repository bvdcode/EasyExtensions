using EasyExtensions.Mediator.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Pipeline
{
    /// <summary>
    /// Behavior for executing all <see cref="IRequestExceptionHandler{TRequest,TResponse,TException}"/> instances
    ///     after an exception is thrown by the following pipeline steps
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestExceptionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the RequestExceptionProcessorBehavior class using the specified service
        /// provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required by the behavior. Cannot be null.</param>
        public RequestExceptionProcessorBehavior(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        /// <summary>
        /// Handles the request by invoking the next handler in the pipeline and processes any exceptions using
        /// registered exception handlers.
        /// </summary>
        /// <remarks>If an exception occurs during request processing, registered exception handlers are
        /// invoked in order of specificity. If an exception is handled and a response is provided, that response is
        /// returned; otherwise, the original exception is rethrown.</remarks>
        /// <param name="request">The request message to be handled. Cannot be null.</param>
        /// <param name="next">A delegate representing the next handler in the pipeline. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by the
        /// handler or an exception handler.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an exception handler does not return a Task or if the exception is marked as handled but no
        /// response is provided.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var state = new RequestExceptionHandlerState<TResponse>();

                var exceptionTypes = GetExceptionTypes(exception.GetType());

                var handlersForException = exceptionTypes
                    .SelectMany(exceptionType => GetHandlersForException(exceptionType, request))
                    .GroupBy(handlerForException => handlerForException.Handler.GetType())
                    .Select(handlerForException => handlerForException.First())
                    .Select(handlerForException => (MethodInfo: GetMethodInfoForHandler(handlerForException.ExceptionType), handlerForException.Handler))
                    .ToList();

                foreach (var handlerForException in handlersForException)
                {
                    try
                    {
                        await ((Task)(handlerForException.MethodInfo.Invoke(handlerForException.Handler, new object[] { request, exception, state, cancellationToken })
                                       ?? throw new InvalidOperationException("Did not return a Task from the exception handler."))).ConfigureAwait(false);
                    }
                    catch (TargetInvocationException invocationException) when (invocationException.InnerException != null)
                    {
                        // Unwrap invocation exception to throw the actual error
                        ExceptionDispatchInfo.Capture(invocationException.InnerException).Throw();
                    }

                    if (state.Handled)
                    {
                        break;
                    }
                }

                if (!state.Handled)
                {
                    throw;
                }

                if (state.Response is null)
                {
                    throw;
                }

                return state.Response; //cannot be null if Handled
            }
        }
        private static IEnumerable<Type> GetExceptionTypes(Type? exceptionType)
        {
            while (exceptionType != null && exceptionType != typeof(object))
            {
                yield return exceptionType;
                exceptionType = exceptionType.BaseType;
            }
        }

        private IEnumerable<(Type ExceptionType, object Handler)> GetHandlersForException(Type exceptionType, TRequest request)
        {
            var exceptionHandlerInterfaceType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(typeof(TRequest), typeof(TResponse), exceptionType);
            var enumerableExceptionHandlerInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionHandlerInterfaceType);
            var exceptionHandlers = (IEnumerable<object>)_serviceProvider.GetRequiredService(enumerableExceptionHandlerInterfaceType);
            return HandlersOrderer.Prioritize(exceptionHandlers.ToList(), request)
                .Select(handler => (exceptionType, action: handler));
        }

        private static MethodInfo GetMethodInfoForHandler(Type exceptionType)
        {
            var exceptionHandlerInterfaceType = typeof(IRequestExceptionHandler<,,>).MakeGenericType(typeof(TRequest), typeof(TResponse), exceptionType);
            return exceptionHandlerInterfaceType.GetMethod(nameof(IRequestExceptionHandler<TRequest, TResponse, Exception>.Handle))
                ?? throw new InvalidOperationException($"Could not find method {nameof(IRequestExceptionHandler<TRequest, TResponse, Exception>.Handle)} on type {exceptionHandlerInterfaceType}");
        }
    }
}