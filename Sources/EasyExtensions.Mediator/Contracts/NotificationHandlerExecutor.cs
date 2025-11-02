using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Provides a container for an asynchronous notification handler and its associated delegate.
    /// </summary>
    /// <remarks>Use this class to encapsulate a notification handler instance along with a delegate that
    /// processes notifications asynchronously. This enables flexible assignment and execution of notification handling
    /// logic, typically within a messaging or event-driven system.</remarks>
    public class NotificationHandlerExecutor
    {
        /// <summary>
        /// Gets the instance of the handler associated with this object.
        /// </summary>
        public object HandlerInstance { get; } = null!;

        /// <summary>
        /// Gets the delegate that handles notification processing asynchronously.
        /// </summary>
        /// <remarks>The delegate accepts an <see cref="INotification"/> instance and a <see
        /// cref="CancellationToken"/>, and returns a <see cref="Task"/> representing the asynchronous operation. Assign
        /// this property to specify custom logic for handling notifications.</remarks>
        public Func<INotification, CancellationToken, Task>? HandlerCallback { get; }
    }
}