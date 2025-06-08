using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapstagram.Models;

public class Album
{
    public int Id { get; set; }
    
    [Required] 
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey("UserId")] 
    public virtual User User { get; set; } = null!;
    
    [Required, StringLength(100)] 
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)] 
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsPrivate { get; set; } = false;
    
    public string? CoverPhotoUrl { get; set; }
    
    // Navigation properties
    public virtual ICollection<Photo> Photos { get; set; } = [];
}

public class Photo
{
    public int Id { get; set; }
    
    public int AlbumId { get; set; }
    
    [ForeignKey("AlbumId")] 
    public virtual Album Album { get; set; } = null!;
    
    [Required] 
    public string ImageUrl { get; set; } = string.Empty;
    
    [StringLength(500)] 
    public string? Caption { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<PhotoComment> Comments { get; set; } = [];
    public virtual ICollection<PhotoLike> Likes { get; set; } = [];
}

public class PhotoComment
{
    public int Id { get; set; }
    
    public int PhotoId { get; set; }
    
    [ForeignKey("PhotoId")]
    [DeleteBehavior(DeleteBehavior.NoAction)] 
    public virtual Photo Photo { get; set; } = null!;
    
    [Required] 
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual User User { get; set; } = null!;
    
    [Required, StringLength(500)] 
    public string Text { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Comment approval system
    public bool IsApproved { get; set; } = false;
    
    public DateTime? ApprovedAt { get; set; }
}

public class PhotoLike
{
    public int Id { get; set; }
    
    public int PhotoId { get; set; }
    
    [ForeignKey("PhotoId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual Photo Photo { get; set; } = null!;
    
    [Required] 
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey("UserId")]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public virtual User User { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
