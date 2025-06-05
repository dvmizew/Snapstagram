using Snapstagram.Services;

namespace Snapstagram.BackgroundServices;

public class NotificationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationCleanupService> _logger;

    public NotificationCleanupService(IServiceProvider serviceProvider, ILogger<NotificationCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

                await notificationService.DeleteOldNotificationsAsync(30); // Delete notifications older than 30 days
                
                _logger.LogInformation("Notification cleanup completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during notification cleanup");
            }

            // Run cleanup once per day
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
