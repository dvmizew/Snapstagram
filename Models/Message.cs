namespace Snapstagram.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }
        public string? RecipientId { get; set; }
        public ApplicationUser? Recipient { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public string? DeleteReason { get; set; }
        public string? DeletedByUserId { get; set; }
        public ApplicationUser? DeletedByUser { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}