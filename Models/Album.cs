using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Models
{
    public class Album
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        
        // Navigation property
        public ICollection<AlbumPhoto> Photos { get; set; } = new List<AlbumPhoto>();
    }
}
