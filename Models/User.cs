using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Models;

public class User : IdentityUser
{
    [Required, StringLength(50)] public string DisplayName { get; set; } = string.Empty;
    [StringLength(500)] public string Bio { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPrivate { get; set; } = false;

    // Navigation properties
    public virtual ICollection<Post> Posts { get; set; } = [];
    public virtual ICollection<Story> Stories { get; set; } = [];
    public virtual ICollection<Follow> Followers { get; set; } = [];
    public virtual ICollection<Follow> Following { get; set; } = [];
    public virtual ICollection<Message> SentMessages { get; set; } = [];
    public virtual ICollection<Message> ReceivedMessages { get; set; } = [];
    public virtual ICollection<Notification> Notifications { get; set; } = [];
    public virtual ICollection<Notification> TriggeredNotifications { get; set; } = [];
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = [];
}
