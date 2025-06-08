using System;

namespace Snapstagram.Models
{
    public class AlbumPhotoLike
    {
        public int Id { get; set; }
        public int PhotoId { get; set; }
        public string UserId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public AlbumPhoto Photo { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
    }
}
