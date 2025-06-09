namespace Snapstagram.Models
{
    public class GroupMessage
    {
        public int Id { get; set; }
        public int ChatGroupId { get; set; }
        public ChatGroup? ChatGroup { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser? Sender { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string? DeleteReason { get; set; }
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        public DateTime? DeletedAt { get; set; }
        public List<GroupMessageRead> ReadByMembers { get; set; } = new List<GroupMessageRead>();
    }
}
