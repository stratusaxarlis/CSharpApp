using MediatR;

namespace CSharpApp.Application.Common.Publisher;

public sealed class ParallelNoWaitPublisher: INotificationPublisher
{
    public Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        foreach (NotificationHandlerExecutor handler in handlerExecutors)
            Task.Run(() => handler.HandlerCallback(notification, cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }
}
