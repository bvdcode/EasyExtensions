using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using EasyExtensions.Mediator.Contracts;

namespace EasyExtensions.Mediator
{
    public interface INotificationPublisher
    {
        Task Publish(IEnumerable<NotificationHandlerExecutor> handlerExecutors, INotification notification,
            CancellationToken cancellationToken);
    }
}