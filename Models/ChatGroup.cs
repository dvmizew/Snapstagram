namespace Snapstagram.Models
{
    public class ChatGroup
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? GroupImageUrl { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser? CreatedByUser { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Navigation properties
        public List<ChatGroupMember> Members { get; set; } = new List<ChatGroupMember>();
        public List<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
    }
}
