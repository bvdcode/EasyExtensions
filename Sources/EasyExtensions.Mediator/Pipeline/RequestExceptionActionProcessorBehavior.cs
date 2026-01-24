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
    /// Behavior for executing all <see cref="IRequestExceptionAction{TRequest,TException}"/> instances
    ///     after an exception is thrown by the following pipeline steps
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    public class RequestExceptionActionProcessorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the RequestExceptionActionProcessorBehavior class using the specified service
        /// provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies required by the behavior. Cannot be null.</param>
        public RequestExceptionActionProcessorBehavior(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        /// <summary>
        /// Handles the specified request by invoking the next handler in the pipeline and applying any registered
        /// exception actions if an exception occurs.
        /// </summary>
        /// <remarks>If an exception is thrown during the execution of the next handler, any registered
        /// exception actions for the exception type are invoked before the exception is rethrown. Exception actions are
        /// executed in the order they are registered and may themselves throw exceptions.</remarks>
        /// <param name="request">The request message to process. Cannot be null.</param>
        /// <param name="next">A delegate representing the next handler in the pipeline to invoke. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the response produced by the
        /// handler pipeline.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an exception action method cannot be invoked or does not return a valid task.</exception>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var exceptionTypes = GetExceptionTypes(exception.GetType());

                var actionsForException = exceptionTypes
                    .SelectMany(exceptionType => GetActionsForException(exceptionType, request))
                    .GroupBy(actionForException => actionForException.Action.GetType())
                    .Select(actionForException => actionForException.First())
                    .Select(actionForException => (MethodInfo: GetMethodInfoForAction(actionForException.ExceptionType), actionForException.Action))
                    .ToList();

                foreach (var actionForException in actionsForException)
                {
                    try
                    {
                        await ((Task)(actionForException.MethodInfo.Invoke(actionForException.Action, new object[] { request, exception, cancellationToken })
                                      ?? throw new InvalidOperationException($"Could not create task for action method {actionForException.MethodInfo}."))).ConfigureAwait(false);
                    }
                    catch (TargetInvocationException invocationException) when (invocationException.InnerException != null)
                    {
                        // Unwrap invocation exception to throw the actual error
                        ExceptionDispatchInfo.Capture(invocationException.InnerException).Throw();
                    }
                }

                throw;
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

        private IEnumerable<(Type ExceptionType, object Action)> GetActionsForException(Type exceptionType, TRequest request)
        {
            var exceptionActionInterfaceType = typeof(IRequestExceptionAction<,>).MakeGenericType(typeof(TRequest), exceptionType);
            var enumerableExceptionActionInterfaceType = typeof(IEnumerable<>).MakeGenericType(exceptionActionInterfaceType);

            var actionsForException = (IEnumerable<object>)_serviceProvider.GetRequiredService(enumerableExceptionActionInterfaceType);

            return HandlersOrderer.Prioritize(actionsForException.ToList(), request)
                .Select(action => (exceptionType, action));
        }

        private static MethodInfo GetMethodInfoForAction(Type exceptionType)
        {
            var exceptionActionInterfaceType = typeof(IRequestExceptionAction<,>).MakeGenericType(typeof(TRequest), exceptionType);

            var actionMethodInfo =
                exceptionActionInterfaceType.GetMethod(nameof(IRequestExceptionAction<TRequest, Exception>.Execute))
                ?? throw new InvalidOperationException(
                    $"Could not find method {nameof(IRequestExceptionAction<TRequest, Exception>.Execute)} on type {exceptionActionInterfaceType}");

            return actionMethodInfo;
        }
    }
}