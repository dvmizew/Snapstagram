using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snapstagram.Models;

public class Follow
{
    public int Id { get; set; }
    [Required] public string FollowerId { get; set; } = string.Empty;
    [ForeignKey("FollowerId")] public virtual User Follower { get; set; } = null!;
    [Required] public string FollowingId { get; set; } = string.Empty;
    [ForeignKey("FollowingId")] public virtual User Following { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Like
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    public int PostId { get; set; }
    [ForeignKey("PostId")] public virtual Post Post { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Comment
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    public int PostId { get; set; }
    [ForeignKey("PostId")] public virtual Post Post { get; set; } = null!;
    [Required, StringLength(500)] public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class StoryView
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    public int StoryId { get; set; }
    [ForeignKey("StoryId")] public virtual Story Story { get; set; } = null!;
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}

public class Message
{
    public int Id { get; set; }
    [Required] public string SenderId { get; set; } = string.Empty;
    [ForeignKey("SenderId")] public virtual User Sender { get; set; } = null!;
    [Required] public string ReceiverId { get; set; } = string.Empty;
    [ForeignKey("ReceiverId")] public virtual User Receiver { get; set; } = null!;
    [Required, StringLength(1000)] public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}

public class Bookmark
{
    public int Id { get; set; }
    [Required] public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")] public virtual User User { get; set; } = null!;
    public int PostId { get; set; }
    [ForeignKey("PostId")] public virtual Post Post { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


