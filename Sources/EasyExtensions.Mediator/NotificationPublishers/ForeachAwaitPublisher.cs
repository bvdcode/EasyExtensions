using EasyExtensions.Mediator.Contracts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.NotificationPublishers
{
    /// <summary>
    /// Awaits each notification handler in a single foreach loop:
    /// <code>
    /// foreach (var handler in handlers) {
    ///     await handler(notification, cancellationToken);
    /// }
    /// </code>
    /// </summary>
    public class ForeachAwaitPublisher : INotificationPublisher
    {
        /// <summary>
        /// Publishes a notification to all specified notification handler executors asynchronously.
        /// </summary>
        /// <remarks>Each handler in the collection is invoked in sequence. The operation completes when
        /// all handlers have finished processing the notification. If the cancellation token is triggered, the
        /// operation may be canceled before all handlers are invoked.</remarks>
        /// <param name="handlerExecutors">A collection of notification handler executors that will process the notification. Cannot be null.</param>
        /// <param name="notification">The notification to be published to each handler. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        public async Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        {
            foreach (var handler in handlerExecutors)
            {
                await handler.HandlerCallback(notification, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}