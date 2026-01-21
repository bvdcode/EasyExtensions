using EasyExtensions.Mediator.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator
{
    /// <summary>
    /// Encapsulates a notification handler instance and its associated execution callback.
    /// </summary>
    /// <remarks>This class is typically used to store and invoke notification handlers in a decoupled manner,
    /// allowing for flexible execution of notification processing logic. It is commonly used in messaging or
    /// event-driven architectures where notifications are dispatched to multiple handlers.</remarks>
    public class NotificationHandlerExecutor
    {
        /// <summary>
        /// Gets the instance of the handler associated with this object.
        /// </summary>
        public object HandlerInstance { get; }

        /// <summary>
        /// Gets the callback function that handles incoming notifications asynchronously.
        /// </summary>
        /// <remarks>The callback is invoked with the notification to process and a cancellation token
        /// that can be used to observe cancellation requests. The function should return a task that completes when the
        /// notification has been handled.</remarks>
        public Func<INotification, CancellationToken, Task> HandlerCallback { get; }

        /// <summary>
        /// Initializes a new instance of the NotificationHandlerExecutor class with the specified handler instance and
        /// callback.
        /// </summary>
        /// <param name="handlerInstance">The object instance that contains the notification handler logic. Cannot be null.</param>
        /// <param name="handlerCallback">A delegate that represents the asynchronous callback to invoke when handling a notification. Cannot be null.</param>
        public NotificationHandlerExecutor(object handlerInstance, Func<INotification, CancellationToken, Task> handlerCallback)
        {
            HandlerInstance = handlerInstance;
            HandlerCallback = handlerCallback;
        }
    }
}