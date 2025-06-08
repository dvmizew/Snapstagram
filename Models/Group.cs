using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Models;

public class Group
{
    public int Id { get; set; }
    [Required, StringLength(50)] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public virtual ICollection<User> Members { get; set; } = new List<User>();
}
