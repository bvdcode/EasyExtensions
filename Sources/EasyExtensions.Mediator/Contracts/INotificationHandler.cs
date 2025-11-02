using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator.Contracts
{
    /// <summary>
    /// Defines a handler for processing notification messages of a specified type asynchronously.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to handle. Must implement the <see cref="INotification"/> interface.</typeparam>
    public interface INotificationHandler<in TNotification> where TNotification : INotification
    {
        /// <summary>
        /// Handles the specified notification asynchronously.
        /// </summary>
        /// <param name="notification">The notification to process. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task Handle(TNotification notification, CancellationToken cancellationToken = default);
    }
}
