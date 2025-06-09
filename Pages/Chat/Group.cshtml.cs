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
    public class GroupModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public GroupModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ChatGroup? Group { get; set; }
        public string? CurrentUserId { get; set; }
        public GroupMemberRole CurrentUserRole { get; set; }
        public List<ChatGroupMember> Members { get; set; } = new();
        public List<GroupMessage> Messages { get; set; } = new();
        public List<ApplicationUser> AvailableFriends { get; set; } = new();
        public Dictionary<int, int> MessageReadCounts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int groupId)
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (CurrentUserId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Get the group
            Group = await _context.ChatGroups
                .Include(g => g.CreatedByUser)
                .FirstOrDefaultAsync(g => g.Id == groupId && g.IsActive && !g.IsDeleted);

            if (Group == null)
            {
                return NotFound();
            }

            // Check if current user is a member
            var currentUserMember = await _context.ChatGroupMembers
                .FirstOrDefaultAsync(m => m.ChatGroupId == groupId && m.UserId == CurrentUserId && m.IsActive);

            if (currentUserMember == null)
            {
                return RedirectToPage("/Chat/Index");
            }

            CurrentUserRole = currentUserMember.Role;

            // Get all active members
            Members = await _context.ChatGroupMembers
                .Where(m => m.ChatGroupId == groupId && m.IsActive)
                .Include(m => m.User)
                .OrderBy(m => m.Role)
                .ThenBy(m => m.User!.FirstName)
                .ToListAsync();

            // Get group messages
            Messages = await _context.GroupMessages
                .Where(m => m.ChatGroupId == groupId && !m.IsDeleted)
                .Include(m => m.Sender)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Get read counts for each message
            foreach (var message in Messages)
            {
                var readCount = await _context.GroupMessageReads
                    .CountAsync(r => r.GroupMessageId == message.Id);
                MessageReadCounts[message.Id] = readCount;
            }

            // Mark messages as read for current user
            var unreadMessageIds = Messages
                .Where(m => m.SenderId != CurrentUserId)
                .Where(m => !_context.GroupMessageReads.Any(r => r.GroupMessageId == m.Id && r.UserId == CurrentUserId))
                .Select(m => m.Id)
                .ToList();

            foreach (var messageId in unreadMessageIds)
            {
                var readStatus = new GroupMessageRead
                {
                    GroupMessageId = messageId,
                    UserId = CurrentUserId,
                    ReadAt = DateTime.UtcNow
                };
                _context.GroupMessageReads.Add(readStatus);
            }

            if (unreadMessageIds.Any())
            {
                await _context.SaveChangesAsync();
            }

            // Get friends who are not already in the group (for adding members)
            if (CurrentUserRole == GroupMemberRole.Owner || CurrentUserRole == GroupMemberRole.Admin)
            {
                var memberUserIds = Members.Select(m => m.UserId).ToList();
                var friendIds = await _context.FriendRequests
                    .Where(fr => (fr.SenderId == CurrentUserId || fr.ReceiverId == CurrentUserId) && fr.Status == FriendRequestStatus.Accepted)
                    .Select(fr => fr.SenderId == CurrentUserId ? fr.ReceiverId : fr.SenderId)
                    .ToListAsync();

                AvailableFriends = await _context.Users
                    .Where(u => friendIds.Contains(u.Id) && !memberUserIds.Contains(u.Id))
                    .ToListAsync();
            }

            return Page();
        }

        public int GetReadCount(int messageId)
        {
            return MessageReadCounts.TryGetValue(messageId, out var count) ? count : 0;
        }
    }
}
