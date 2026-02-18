using EasyExtensions.Mediator.Contracts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator
{
    /// <summary>
    /// Defines a contract for publishing notifications to one or more notification handler executors asynchronously.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for invoking the provided notification
    /// handler executors with the specified notification. The publishing process is typically asynchronous and may
    /// involve invoking handlers in parallel or sequentially, depending on the implementation.</remarks>
    public interface INotificationPublisher
    {
        /// <summary>
        /// Publishes a notification to the specified handler executors asynchronously.
        /// </summary>
        /// <param name="handlerExecutors">A collection of handler executors that will process the notification. Cannot be null.</param>
        /// <param name="notification">The notification instance to be published. Cannot be null.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
            CancellationToken cancellationToken);
    }
}