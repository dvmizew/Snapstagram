using System;

namespace Snapstagram.Models
{
    public class AlbumPhotoComment
    {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public string UserId { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByUserId { get; set; }

        // Navigation properties
        public AlbumPhoto Photo { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
    }
}
