namespace Snapstagram.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }
        public string? ReceiverId { get; set; }
        public ApplicationUser? Receiver { get; set; }
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
        public string? Message { get; set; } // Optional message with the request
    }

    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Rejected,
        Cancelled
    }
}
