using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using System.Security.Claims;

namespace Snapstagram.Pages.Chat
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ConversationViewModel> RecentConversations { get; set; } = new();
        public List<GroupViewModel> UserGroups { get; set; } = new();
        public List<ApplicationUser> Friends { get; set; } = new();

        public async Task OnGetAsync()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null) return;

            // Get user's friends
            var friendIds = await _context.FriendRequests
                .Where(fr => (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId) && fr.Status == FriendRequestStatus.Accepted)
                .Select(fr => fr.SenderId == currentUserId ? fr.ReceiverId : fr.SenderId)
                .ToListAsync();

            Friends = await _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .ToListAsync();

            // Get recent individual conversations
            var conversations = await _context.Messages
                .Where(m => m.SenderId == currentUserId || m.RecipientId == currentUserId)
                .GroupBy(m => m.SenderId == currentUserId ? m.RecipientId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(m => m.SentAt).First(),
                    UnreadCount = g.Count(m => m.RecipientId == currentUserId && !m.IsRead)
                })
                .ToListAsync();

            var otherUserIds = conversations.Select(c => c.OtherUserId).ToList();
            var otherUsers = await _context.Users
                .Where(u => otherUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            RecentConversations = conversations
                .Where(c => c.OtherUserId != null && otherUsers.ContainsKey(c.OtherUserId))
                .Select(c => new ConversationViewModel
                {
                    Id = 0, // We'll implement proper conversation IDs later
                    OtherUser = otherUsers[c.OtherUserId!],
                    LastMessage = c.LastMessage.Content,
                    LastMessageAt = c.LastMessage.SentAt,
                    UnreadCount = c.UnreadCount
                })
                .OrderByDescending(c => c.LastMessageAt)
                .ToList();

            // Get user's groups
            var userGroupIds = await _context.ChatGroupMembers
                .Where(m => m.UserId == currentUserId && m.IsActive)
                .Select(m => m.ChatGroupId)
                .ToListAsync();

            var groups = await _context.ChatGroups
                .Where(g => userGroupIds.Contains(g.Id) && g.IsActive && !g.IsDeleted)
                .ToListAsync();

            UserGroups = new List<GroupViewModel>();

            foreach (var group in groups)
            {
                var lastMessage = await _context.GroupMessages
                    .Where(m => m.ChatGroupId == group.Id && !m.IsDeleted)
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.GroupMessages
                    .Where(m => m.ChatGroupId == group.Id && !m.IsDeleted)
                    .Where(m => !_context.GroupMessageReads.Any(r => r.GroupMessageId == m.Id && r.UserId == currentUserId))
                    .CountAsync();

                UserGroups.Add(new GroupViewModel
                {
                    Id = group.Id,
                    Name = group.Name,
                    LastMessage = lastMessage?.Content ?? "No messages yet",
                    LastMessageAt = lastMessage?.SentAt,
                    UnreadCount = unreadCount
                });
            }

            UserGroups = UserGroups.OrderByDescending(g => g.LastMessageAt).ToList();
        }
    }

    public class ConversationViewModel
    {
        public int Id { get; set; }
        public ApplicationUser? OtherUser { get; set; }
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class GroupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
