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
    public class ConversationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ConversationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public ApplicationUser? OtherUser { get; set; }
        public string? CurrentUserId { get; set; }
        public List<Message> Messages { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (CurrentUserId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Chat/Index");
            }

            // Get the other user
            OtherUser = await _context.Users.FindAsync(userId);
            if (OtherUser == null)
            {
                return NotFound();
            }

            // Check if they are friends
            var areFriends = await _context.FriendRequests
                .AnyAsync(fr => 
                    ((fr.SenderId == CurrentUserId && fr.ReceiverId == userId) ||
                     (fr.SenderId == userId && fr.ReceiverId == CurrentUserId)) &&
                    fr.Status == FriendRequestStatus.Accepted);

            if (!areFriends)
            {
                return RedirectToPage("/Chat/Index");
            }

            // Get messages between the two users
            Messages = await _context.Messages
                .Where(m => 
                    (m.SenderId == CurrentUserId && m.RecipientId == userId) ||
                    (m.SenderId == userId && m.RecipientId == CurrentUserId))
                .Where(m => !m.IsDeleted)
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Mark messages from the other user as read
            var unreadMessages = Messages
                .Where(m => m.SenderId == userId && m.RecipientId == CurrentUserId && !m.IsRead)
                .ToList();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Page();
        }
    }
}
