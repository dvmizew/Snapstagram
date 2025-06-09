namespace Snapstagram.Models
{
    public class GroupMessageRead
    {
        public int Id { get; set; }
        public int GroupMessageId { get; set; }
        public GroupMessage? GroupMessage { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}
