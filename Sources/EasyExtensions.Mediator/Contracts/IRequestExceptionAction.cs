using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines an action to be performed when an exception occurs during the processing of a request.
    /// </summary>
    /// <remarks>Implement this interface to provide custom exception handling logic for specific request and
    /// exception types. The action is typically invoked by a request processing pipeline or middleware when an
    /// exception of the specified type occurs.</remarks>
    /// <typeparam name="TRequest">The type of the request associated with the exception. This type must not be null.</typeparam>
    /// <typeparam name="TException">The type of exception that triggered the action. This type must derive from Exception.</typeparam>
    public interface IRequestExceptionAction<in TRequest, in TException> where TRequest : notnull where TException : Exception
    {
        /// <summary>
        /// Executes the operation using the specified request and exception context.
        /// </summary>
        /// <param name="request">The request object that provides the necessary data for the operation.</param>
        /// <param name="exception">The exception instance that triggered the execution. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous execution operation.</returns>
        Task Execute(TRequest request, TException exception, CancellationToken cancellationToken = default);
    }
}
