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
        public bool IsProfilePublic { get; set; } = true;
        
        // Additional profile fields
        public DateTime? DateOfBirth { get; set; }
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string? Occupation { get; set; }
    }

    public static class UserRoles
    {
        public const string Visitor = "Visitor";                 // Unregistered visitor
        public const string RegisteredUser = "RegisteredUser";    // Registered user
        public const string Administrator = "Administrator";      // Administrator
    }
}