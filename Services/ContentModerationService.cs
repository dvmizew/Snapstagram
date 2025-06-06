using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Services
{
    public class ContentModerationService
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly ILogger<ContentModerationService> _logger;

        public ContentModerationService(
            ApplicationDbContext context,
            NotificationService notificationService,
            ILogger<ContentModerationService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<List<Post>> GetReportedPostsAsync()
        {
            // In a real app, we'd have a ReportedContent table. For now, we'll just get all posts
            return await _context.Posts
                .Include(p => p.User)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetReportedCommentsAsync()
        {
            // In a real app, we'd have a ReportedContent table. For now, we'll just get all comments
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<Message>> GetReportedMessagesAsync()
        {
            // In a real app, we'd have a ReportedContent table. For now, we'll just get all messages
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .Take(20)
                .ToListAsync();
        }

        public async Task RemovePostAsync(int postId, string adminId, string reason)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
            {
                throw new ArgumentException("Post not found");
            }

            post.IsDeleted = true;
            post.DeleteReason = reason;
            post.DeletedByUserId = adminId;
            post.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification to the post owner
            if (post.UserId != null)
            {
                await _notificationService.SendNotificationAsync(
                    post.UserId,
                    $"Your post has been removed. Reason: {reason}",
                    NotificationType.ContentRemoved,
                    postId.ToString()
                );
            }

            _logger.LogInformation($"Post {postId} removed by admin {adminId}. Reason: {reason}");
        }

        public async Task RemoveCommentAsync(int commentId, string adminId, string reason)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            comment.IsDeleted = true;
            comment.DeleteReason = reason;
            comment.DeletedByUserId = adminId;
            comment.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification to the comment owner
            if (comment.UserId != null)
            {
                await _notificationService.SendNotificationAsync(
                    comment.UserId,
                    $"Your comment has been removed. Reason: {reason}",
                    NotificationType.ContentRemoved,
                    commentId.ToString()
                );
            }

            _logger.LogInformation($"Comment {commentId} removed by admin {adminId}. Reason: {reason}");
        }

        public async Task RemoveMessageAsync(int messageId, string adminId, string reason)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                throw new ArgumentException("Message not found");
            }

            message.IsDeleted = true;
            message.DeleteReason = reason;
            message.DeletedByUserId = adminId;
            message.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification to the message sender
            if (message.SenderId != null)
            {
                await _notificationService.SendNotificationAsync(
                    message.SenderId,
                    $"Your message has been removed. Reason: {reason}",
                    NotificationType.ContentRemoved,
                    messageId.ToString()
                );
            }

            _logger.LogInformation($"Message {messageId} removed by admin {adminId}. Reason: {reason}");
        }
    }
}