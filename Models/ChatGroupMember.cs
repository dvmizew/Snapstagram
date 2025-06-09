namespace Snapstagram.Models
{
    public class ChatGroupMember
    {
        public int Id { get; set; }
        public int ChatGroupId { get; set; }
        public ChatGroup? ChatGroup { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? LeftAt { get; set; }
        public string? AddedByUserId { get; set; }
        public ApplicationUser? AddedByUser { get; set; }
    }

    public enum GroupMemberRole
    {
        Member,
        Admin,
        Owner
    }
}
