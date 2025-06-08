using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using System.Security.Claims;

namespace Snapstagram.Pages
{
    [Authorize]
    public class FriendsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FriendsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<User> Friends { get; set; } = [];
        public IList<FriendRequest> ReceivedRequests { get; set; } = [];
        public IList<FriendRequest> SentRequests { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get pending received friend requests
            ReceivedRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == "Pending")
                .Include(fr => fr.Sender)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            // Get pending sent friend requests
            SentRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == userId && fr.Status == "Pending")
                .Include(fr => fr.Receiver)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            // Get all friends (users with accepted friend requests)
            var acceptedReceivedRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == "Accepted")
                .Include(fr => fr.Sender)
                .ToListAsync();

            var acceptedSentRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == userId && fr.Status == "Accepted")
                .Include(fr => fr.Receiver)
                .ToListAsync();

            // Combine the lists of friends
            Friends = acceptedReceivedRequests.Select(fr => fr.Sender)
                .Concat(acceptedSentRequests.Select(fr => fr.Receiver))
                .ToList();

            return Page();
        }
    }
}
