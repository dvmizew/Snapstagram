using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snapstagram.Models;

public class Post
{
    public int Id { get; set; }

    [Required] public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;

    [StringLength(2000)] public string Caption { get; set; } = string.Empty;
    [Required] public string ImageUrl { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }

    public virtual ICollection<Like> Likes { get; set; } = [];
    public virtual ICollection<Comment> Comments { get; set; } = [];
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = [];
}
