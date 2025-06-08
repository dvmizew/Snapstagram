using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Models
{
    public class AlbumPhoto
    {
        public int Id { get; set; }
        
        public int AlbumId { get; set; }
        public Album? Album { get; set; }
        
        [Required]
        public string Url { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Name { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [StringLength(256)]
        public string? Description { get; set; }
        
        public int OrderIndex { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }

        public ICollection<AlbumPhotoLike> Likes { get; set; } = new List<AlbumPhotoLike>();
        public ICollection<AlbumPhotoComment> Comments { get; set; } = new List<AlbumPhotoComment>();
    }
}
