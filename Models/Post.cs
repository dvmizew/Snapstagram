using Microsoft.AspNetCore.Identity;

namespace Snapstagram.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string? ImageUrl { get; set; }
        public string Caption { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int ShareCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string? DeleteReason { get; set; }
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}