namespace Snapstagram.Models
{
    public class Conversation
    {
        public int Id { get; set; }
        public string User1Id { get; set; } = string.Empty;
        public ApplicationUser? User1 { get; set; }
        public string User2Id { get; set; } = string.Empty;
        public ApplicationUser? User2 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
