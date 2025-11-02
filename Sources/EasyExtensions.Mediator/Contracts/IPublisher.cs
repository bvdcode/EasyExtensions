using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a contract for publishing notifications to registered handlers asynchronously.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for delivering notifications to all
    /// appropriate handlers. The order in which handlers are invoked is not guaranteed. Cancellation may prevent some
    /// handlers from being invoked. This interface supports both strongly-typed and untyped notifications.</remarks>
    public interface IPublisher
    {
        /// <summary>
        /// Publishes a notification to all registered handlers asynchronously.
        /// </summary>
        /// <param name="notification">The notification object to be published. Cannot be null. The type of the notification determines which
        /// handlers will be invoked.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task Publish(object notification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Publishes a notification to all registered handlers for the specified notification type.
        /// </summary>
        /// <remarks>All handlers registered for the notification type will be invoked. The order in which
        /// handlers are invoked is not guaranteed. If the operation is canceled via the cancellation token, not all
        /// handlers may be invoked.</remarks>
        /// <typeparam name="TNotification">The type of notification to publish. Must implement the INotification interface.</typeparam>
        /// <param name="notification">The notification instance to be published to handlers. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the publish operation.</param>
        /// <returns>A task that represents the asynchronous publish operation.</returns>
        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
    }
}
