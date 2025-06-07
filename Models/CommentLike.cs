namespace Snapstagram.Models
{
    public class CommentLike
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public Comment? Comment { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
