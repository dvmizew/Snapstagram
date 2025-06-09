using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using Snapstagram.Hubs;

namespace Snapstagram.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task SendNotificationAsync(string userId, string message, NotificationType type, string? relatedItemId = null, string? senderName = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                SenderName = senderName,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Type = type,
                RelatedItemId = relatedItemId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Notification sent to user {userId}: {message}");

            // Send real-time notification via SignalR
            try
            {
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    createdAt = notification.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    isRead = notification.IsRead,
                    relatedItemId = notification.RelatedItemId
                });
                
                // Also send updated notification count
                var unreadCount = await GetUnreadNotificationCountAsync(userId);
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateNotificationCount", unreadCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time notification to user {UserId}", userId);
            }
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();

                // Send updated notification count via SignalR
                if (!string.IsNullOrEmpty(notification.UserId))
                {
                    try
                    {
                        var unreadCount = await GetUnreadNotificationCountAsync(notification.UserId);
                        await _hubContext.Clients.Group($"user_{notification.UserId}").SendAsync("UpdateNotificationCount", unreadCount);
                        await _hubContext.Clients.Group($"user_{notification.UserId}").SendAsync("MarkNotificationRead", notificationId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send real-time notification update for user {UserId}", notification.UserId);
                    }
                }
            }
        }

        public async Task MarkAllNotificationsAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            // Send updated notification count via SignalR
            try
            {
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateNotificationCount", 0);
                await _hubContext.Clients.Group($"user_{userId}").SendAsync("MarkAllNotificationsRead");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time notification update for user {UserId}", userId);
            }
        }

        public async Task DeleteNotificationAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                // Send updated notification count via SignalR
                try
                {
                    var unreadCount = await GetUnreadNotificationCountAsync(userId);
                    await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateNotificationCount", unreadCount);
                    await _hubContext.Clients.Group($"user_{userId}").SendAsync("NotificationDeleted", notificationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send real-time notification update for user {UserId}", userId);
                }
            }
        }

        public async Task DeleteAllNotificationsAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notifications.Any())
            {
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();

                // Send updated notification count via SignalR
                try
                {
                    await _hubContext.Clients.Group($"user_{userId}").SendAsync("UpdateNotificationCount", 0);
                    await _hubContext.Clients.Group($"user_{userId}").SendAsync("AllNotificationsDeleted");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send real-time notification update for user {UserId}", userId);
                }
            }
        }
    }
}