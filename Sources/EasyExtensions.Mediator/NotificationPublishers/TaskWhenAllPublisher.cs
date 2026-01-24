using EasyExtensions.Mediator.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.NotificationPublishers
{
    /// <summary>
    /// Uses Task.WhenAll with the list of Handler tasks:
    /// <code>
    /// var tasks = handlers
    ///                .Select(handler => handler.Handle(notification, cancellationToken))
    ///                .ToList();
    /// 
    /// return Task.WhenAll(tasks);
    /// </code>
    /// </summary>
    public class TaskWhenAllPublisher : INotificationPublisher
    {
        /// <summary>
        /// Invokes all notification handler executors for the specified notification and waits for their completion.
        /// </summary>
        /// <param name="handlerExecutors">A collection of handler executors to be invoked for the notification. Cannot be null.</param>
        /// <param name="notification">The notification to be published to each handler. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task completes when all handler executors have
        /// finished processing the notification.</returns>
        public Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken)
        {
            var tasks = handlerExecutors
                .Select(handler => handler.HandlerCallback(notification, cancellationToken))
                .ToArray();

            return Task.WhenAll(tasks);
        }
    }
}