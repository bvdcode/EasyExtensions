using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyExtensions.Mediator.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EasyExtensions.Mediator.Wrappers
{
    /// <summary>
    /// Provides an abstract base class for wrapping notification handler execution logic within a publish pipeline.
    /// </summary>
    /// <remarks>Implementations of this class are used to customize how notifications are handled and
    /// dispatched to their respective handlers. This type is typically used internally by notification publishing
    /// mechanisms to coordinate handler invocation and manage dependencies.</remarks>
    public abstract class NotificationHandlerWrapper
    {
        /// <summary>
        /// Handles the specified notification using the provided service factory and publish delegate.
        /// </summary>
        /// <param name="notification">The notification instance to be processed. Cannot be null.</param>
        /// <param name="serviceFactory">A service provider used to resolve dependencies required for handling the notification. Cannot be null.</param>
        /// <param name="publish">A delegate that publishes the notification to a collection of notification handler executors. The delegate
        /// receives the handlers, the notification, and a cancellation token.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the notification handling operation.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        public abstract Task Handle(INotification notification, IServiceProvider serviceFactory,
            Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides a wrapper for handling notifications of a specific type by invoking all registered notification
    /// handlers for that type.
    /// </summary>
    /// <remarks>This class is typically used internally by notification publishing mechanisms to coordinate
    /// the invocation of multiple <see cref="INotificationHandler{TNotification}"/> implementations for a given
    /// notification type. It enables extensibility and decouples the notification dispatching logic from the concrete
    /// handler implementations.</remarks>
    /// <typeparam name="TNotification">The type of notification to handle. Must implement <see cref="INotification"/>.</typeparam>
    public class NotificationHandlerWrapperImpl<TNotification> : NotificationHandlerWrapper
        where TNotification : INotification
    {
        /// <summary>
        /// Invokes all notification handlers for the specified notification using the provided publish delegate.
        /// </summary>
        /// <remarks>This method resolves all handlers for the notification type from the service provider
        /// and passes them to the publish delegate for execution. The actual invocation of handlers is determined by
        /// the provided publish delegate.</remarks>
        /// <param name="notification">The notification instance to be handled. Cannot be null.</param>
        /// <param name="serviceFactory">The service provider used to resolve notification handler instances.</param>
        /// <param name="publish">A delegate that publishes the notification to the resolved handlers. Receives the collection of handler
        /// executors, the notification, and a cancellation token.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous handling operation.</returns>
        public override Task Handle(INotification notification, IServiceProvider serviceFactory,
            Func<IEnumerable<NotificationHandlerExecutor>, INotification, CancellationToken, Task> publish,
            CancellationToken cancellationToken)
        {
            var handlers = serviceFactory
                .GetServices<INotificationHandler<TNotification>>()
                .Select(x => new NotificationHandlerExecutor(x, (theNotification, theToken) => x.Handle((TNotification)theNotification, theToken)));

            return publish(handlers, notification, cancellationToken);
        }
    }
}