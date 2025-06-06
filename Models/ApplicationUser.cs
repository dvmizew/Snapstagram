using Microsoft.AspNetCore.Identity;

namespace Snapstagram.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Bio { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public static class UserRoles
    {
        public const string Visitor = "Visitor";
        public const string RegisteredUser = "RegisteredUser";
        public const string Moderator = "Moderator";
        public const string Administrator = "Administrator";
    }
}