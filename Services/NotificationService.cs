using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateLikeNotificationAsync(string userId, string actorId, int postId)
    {
        // Don't notify if the user liked their own post
        if (userId == actorId) return;

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            Type = NotificationType.Like,
            PostId = postId,
            Message = "liked your post"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateCommentNotificationAsync(string userId, string actorId, int postId, int commentId)
    {
        // Don't notify if the user commented on their own post
        if (userId == actorId) return;

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            Type = NotificationType.Comment,
            PostId = postId,
            CommentId = commentId,
            Message = "commented on your post"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateFollowNotificationAsync(string userId, string actorId)
    {
        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            Type = NotificationType.Follow,
            Message = "started following you"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateMessageNotificationAsync(string userId, string actorId, int messageId)
    {
        // Don't notify if the user sent a message to themselves
        if (userId == actorId) return;

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            Type = NotificationType.Message,
            MessageId = messageId,
            Message = "sent you a message"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateStoryViewNotificationAsync(string userId, string actorId, int storyId)
    {
        // Don't notify if the user viewed their own story
        if (userId == actorId) return;

        var notification = new Notification
        {
            UserId = userId,
            ActorId = actorId,
            Type = NotificationType.StoryView,
            StoryId = storyId,
            Message = "viewed your story"
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<object> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20, string? type = null, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        // Apply filters
        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, out var notificationType))
        {
            query = query.Where(n => n.Type == notificationType);
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync();
        var unreadCount = await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

        // Get paginated results with related data
        var notifications = await query
            .Include(n => n.Actor)
            .Include(n => n.Post)
            .Include(n => n.Comment)
            .Include(n => n.Story)
            .Include(n => n.TargetMessage)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                id = n.Id,
                type = n.Type.ToString(),
                message = n.Message,
                isRead = n.IsRead,
                createdAt = n.CreatedAt,
                readAt = n.ReadAt,
                fromUser = new
                {
                    id = n.Actor!.Id,
                    userName = n.Actor.UserName,
                    displayName = n.Actor.DisplayName,
                    profilePictureUrl = n.Actor.ProfileImageUrl
                },
                postId = n.PostId,
                commentId = n.CommentId,
                storyId = n.StoryId,
                messageId = n.MessageId
            })
            .ToListAsync();

        return new
        {
            success = true,
            data = new
            {
                items = notifications,
                totalCount = totalCount,
                unreadCount = unreadCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            }
        };
    }

    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkNotificationAsReadAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllNotificationsAsReadAsync(string userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteOldNotificationsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var oldNotifications = await _context.Notifications
            .Where(n => n.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();
    }
}