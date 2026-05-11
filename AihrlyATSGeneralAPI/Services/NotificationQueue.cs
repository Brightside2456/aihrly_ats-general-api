using System.Threading.Channels;

namespace AihrlyATSGeneralAPI.Services;

public record NotificationTask(int ApplicationId, string Type);

public interface INotificationQueue
{
    ValueTask QueueNotificationAsync(NotificationTask task);
    IAsyncEnumerable<NotificationTask> DequeueAsync(CancellationToken cancellationToken);
}

public class NotificationQueue : INotificationQueue
{
    private readonly Channel<NotificationTask> _channel;

    public NotificationQueue()
    {
        _channel = Channel.CreateUnbounded<NotificationTask>();
    }

    public async ValueTask QueueNotificationAsync(NotificationTask task)
    {
        await _channel.Writer.WriteAsync(task);
    }

    public IAsyncEnumerable<NotificationTask> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
