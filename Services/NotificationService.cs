using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Services;

public class NotificationService
{
    private readonly ApplicationDbContext context;

    public NotificationService(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task CreateLikeNotificationAsync(string userId, string actorId, int postId)
    {
        if (userId == actorId) return; // Don't notify yourself

        var existingNotification = await context.Notifications
            .FirstOrDefaultAsync(n => n.UserId == userId && 
                                     n.ActorId == actorId && 
                                     n.PostId == postId && 
                                     n.Type == NotificationType.Like);

        if (existingNotification != null) return; // Already exists

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            PostId = postId,
            Type = NotificationType.Like,
            Message = "liked your post"
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task CreateCommentNotificationAsync(string userId, string actorId, int postId, int commentId)
    {
        if (userId == actorId) return; // Don't notify yourself

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            PostId = postId,
            CommentId = commentId,
            Type = NotificationType.Comment,
            Message = "commented on your post"
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task CreateFollowNotificationAsync(string userId, string actorId)
    {
        if (userId == actorId) return; // Don't notify yourself

        var existingNotification = await context.Notifications
            .FirstOrDefaultAsync(n => n.UserId == userId && 
                                     n.ActorId == actorId && 
                                     n.Type == NotificationType.Follow);

        if (existingNotification != null) return; // Already exists

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            Type = NotificationType.Follow,
            Message = "started following you"
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task CreateMessageNotificationAsync(string userId, string actorId, int messageId)
    {
        if (userId == actorId) return; // Don't notify yourself

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            MessageId = messageId,
            Type = NotificationType.Message,
            Message = "sent you a message"
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task CreateStoryViewNotificationAsync(string userId, string actorId, int storyId)
    {
        if (userId == actorId) return; // Don't notify yourself

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            StoryId = storyId,
            Type = NotificationType.StoryView,
            Message = "viewed your story"
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetNotificationsAsync(string userId, int page = 0, int pageSize = 20)
    {
        return await context.Notifications
            .Where(n => n.UserId == userId)
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .Include(n => n.Comment)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(string userId, int notificationId)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(n => n
                .SetProperty(x => x.IsRead, true)
                .SetProperty(x => x.ReadAt, DateTime.UtcNow));
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task DeleteOldNotificationsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        
        await context.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ExecuteDeleteAsync();
    }
}