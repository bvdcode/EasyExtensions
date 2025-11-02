using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a contract for asynchronously publishing notifications to one or more notification handler executors.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for delivering notifications to the
    /// provided handler executors. The publish operation may be subject to cancellation via the supplied cancellation
    /// token. Thread safety and delivery guarantees depend on the specific implementation.</remarks>
    public interface INotificationPublisher
    {
        /// <summary>
        /// Publishes a notification to the specified collection of notification handler executors asynchronously.
        /// </summary>
        /// <param name="handlerExecutors">A collection of executors responsible for handling the notification. Cannot be null.</param>
        /// <param name="notification">The notification instance to be published. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification, CancellationToken cancellationToken = default);
    }
}
