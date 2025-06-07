using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Pages.Account
{
    [Authorize]
    public class FriendsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public FriendsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<ApplicationUser> Friends { get; set; } = new List<ApplicationUser>();
        public List<FriendRequest> PendingRequests { get; set; } = new List<FriendRequest>();
        public List<FriendRequest> SentRequests { get; set; } = new List<FriendRequest>();
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await LoadFriendsDataAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostRespondToFriendRequestAsync(int requestId, bool accept)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == user.Id);

            if (friendRequest == null)
            {
                return new JsonResult(new { success = false, message = "Friend request not found" });
            }

            if (friendRequest.Status != FriendRequestStatus.Pending)
            {
                return new JsonResult(new { success = false, message = "Friend request has already been responded to" });
            }

            friendRequest.Status = accept ? FriendRequestStatus.Accepted : FriendRequestStatus.Rejected;
            friendRequest.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var message = accept ? "Friend request accepted" : "Friend request rejected";
            return new JsonResult(new { success = true, message = message });
        }

        public async Task<IActionResult> OnPostCancelFriendRequestAsync(int requestId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.SenderId == user.Id);

            if (friendRequest == null)
            {
                return new JsonResult(new { success = false, message = "Friend request not found" });
            }

            if (friendRequest.Status != FriendRequestStatus.Pending)
            {
                return new JsonResult(new { success = false, message = "Can only cancel pending friend requests" });
            }

            friendRequest.Status = FriendRequestStatus.Cancelled;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Friend request cancelled" });
        }

        public async Task<IActionResult> OnPostRemoveFriendAsync(string friendId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var friendship = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    ((fr.SenderId == user.Id && fr.ReceiverId == friendId) ||
                     (fr.SenderId == friendId && fr.ReceiverId == user.Id)) &&
                    fr.Status == FriendRequestStatus.Accepted);

            if (friendship == null)
            {
                return new JsonResult(new { success = false, message = "Friendship not found" });
            }

            _context.FriendRequests.Remove(friendship);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Friend removed successfully" });
        }

        private async Task LoadFriendsDataAsync(string userId)
        {
            // Load friends (accepted friend requests)
            var friendships = await _context.FriendRequests
                .Where(fr => 
                    (fr.SenderId == userId || fr.ReceiverId == userId) && 
                    fr.Status == FriendRequestStatus.Accepted)
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .ToListAsync();

            Friends = friendships.Select(fr => 
                fr.SenderId == userId ? fr.Receiver! : fr.Sender!)
                .ToList();

            // Load pending requests (received by current user)
            PendingRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .Include(fr => fr.Sender)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            // Load sent requests (sent by current user)
            SentRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == userId && fr.Status == FriendRequestStatus.Pending)
                .Include(fr => fr.Receiver)
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();
        }
    }
}
