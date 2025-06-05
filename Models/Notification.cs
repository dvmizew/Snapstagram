using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snapstagram.Models;

public enum NotificationType
{
    Like = 1,
    Comment = 2,
    Follow = 3,
    Message = 4,
    StoryView = 5,
    Mention = 6
}

public class Notification
{
    public int Id { get; set; }
    
    [Required] public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    
    [Required] public string? ActorId { get; set; }
    [ForeignKey("ActorId")] public virtual User? Actor { get; set; }
    
    [Required] public NotificationType Type { get; set; }
    
    [StringLength(500)] public string? Message { get; set; }
    
    // Reference to the entity that triggered the notification
    public int? PostId { get; set; }
    [ForeignKey("PostId")] public virtual Post? Post { get; set; }
    
    public int? CommentId { get; set; }
    [ForeignKey("CommentId")] public virtual Comment? Comment { get; set; }
    
    public int? StoryId { get; set; }
    [ForeignKey("StoryId")] public virtual Story? Story { get; set; }
    
    public int? MessageId { get; set; }
    [ForeignKey("MessageId")] public virtual Message? TargetMessage { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}