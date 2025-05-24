using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snapstagram.Models;

public class Story
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    [Required] public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = "image"; // "image" or "video"
    [StringLength(500)] public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    public int ViewsCount { get; set; } = 0;

    // Navigation properties
    public virtual ICollection<StoryView> Views { get; set; } = [];
}
