namespace Snapstagram.Models
{
    public class CommentReply
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public Comment? Comment { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string? DeleteReason { get; set; }
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
