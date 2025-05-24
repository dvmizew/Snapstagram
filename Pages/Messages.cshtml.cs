using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Pages
{
    [Authorize]
    public class MessagesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public MessagesModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<User> RecentChats { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return;

            // Get users that the current user follows or has messaged
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == currentUser.Id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // Get recent chat partners
            var messagedUserIds = await _context.Messages
                .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
                .Select(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            // Combine and get unique user IDs
            var userIds = followingIds.Concat(messagedUserIds).Distinct().ToList();

            RecentChats = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .OrderBy(u => u.DisplayName)
                .ToListAsync();

            // If no recent chats, show some sample users for demo
            if (!RecentChats.Any())
            {
                RecentChats = await _context.Users
                    .Where(u => u.Id != currentUser.Id)
                    .Take(5)
                    .ToListAsync();
            }
        }
    }
}
