namespace Snapstagram.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public NotificationType Type { get; set; }
        public string? RelatedItemId { get; set; } // For linking to the related content (post, comment, message)
    }

    public enum NotificationType
    {
        System,
        ContentRemoved,
        Warning,
        Information,
        Like,
        Comment,
        Reply,
        Follow
    }
}