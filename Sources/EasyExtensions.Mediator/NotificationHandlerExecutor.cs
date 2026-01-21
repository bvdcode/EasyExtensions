using EasyExtensions.Mediator.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyExtensions.Mediator
{
    public class NotificationHandlerExecutor
    {
        public object HandlerInstance { get; }
        public Func<INotification, CancellationToken, Task> HandlerCallback { get; }


        public NotificationHandlerExecutor(object handlerInstance, Func<INotification, CancellationToken, Task> handlerCallback)
        {
            HandlerInstance = handlerInstance;
            HandlerCallback = handlerCallback;
        }
    }
}