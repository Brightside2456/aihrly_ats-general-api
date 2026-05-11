using AihrlyATSGeneralAPI.Data;
using AihrlyATSGeneralAPI.Models.Entities;

namespace AihrlyATSGeneralAPI.Services;

public class NotificationWorker : BackgroundService
{
    private readonly INotificationQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(INotificationQueue queue, IServiceProvider serviceProvider, ILogger<NotificationWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var task in _queue.DequeueAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Sending {Type} notification for application {ApplicationId}...", task.Type, task.ApplicationId);
                
                // Simulate email delay
                await Task.Delay(1000, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AihrlyDbContext>();

                var notification = new Notification
                {
                    ApplicationId = task.ApplicationId,
                    Type = task.Type,
                    SentAt = DateTime.UtcNow
                };

                db.Notifications.Add(notification);
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation("Notification for application {ApplicationId} sent and recorded.", task.ApplicationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification for application {ApplicationId}", task.ApplicationId);
            }
        }
    }
}
