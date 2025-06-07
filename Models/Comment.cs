namespace Snapstagram.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public Post? Post { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string? DeleteReason { get; set; }
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        public List<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
        public List<CommentReply> CommentReplies { get; set; } = new List<CommentReply>();
        
        // Computed properties
        public int LikeCount => CommentLikes?.Count ?? 0;
        public int ReplyCount => CommentReplies?.Count ?? 0;
    }
}