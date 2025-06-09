using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Pages.Account
{
    [Authorize]
    public class NotificationsModel : PageModel
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsModel(NotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public List<Notification> Notifications { get; set; } = new List<Notification>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Notifications = await _notificationService.GetUserNotificationsAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int notificationId)
        {
            await _notificationService.MarkNotificationAsReadAsync(notificationId);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkAllAsReadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _notificationService.MarkAllNotificationsAsReadAsync(user.Id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteNotificationAsync(int notificationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _notificationService.DeleteNotificationAsync(notificationId, user.Id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAllNotificationsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            await _notificationService.DeleteAllNotificationsAsync(user.Id);
            return RedirectToPage();
        }

        public string GetNotificationUrl(Notification notification)
        {
            return notification.Type switch
            {
                NotificationType.FriendRequest or NotificationType.FriendRequestAccepted => "/Account/Friends",
                NotificationType.NewMessage => !string.IsNullOrEmpty(notification.RelatedItemId) 
                    ? $"/Chat/Conversation?userId={notification.RelatedItemId}" 
                    : "/Chat/Index",
                NotificationType.NewGroupMessage => !string.IsNullOrEmpty(notification.RelatedItemId)
                    ? $"/Chat/Group?groupId={notification.RelatedItemId}"
                    : "/Chat/Index",
                NotificationType.Like or NotificationType.Comment or NotificationType.Reply => 
                    // For likes and comments, RelatedItemId could be the user who posted or the post owner
                    // Navigate to the sender's profile if available, otherwise to feed
                    !string.IsNullOrEmpty(notification.RelatedItemId)
                        ? $"/Account/Profile?id={notification.RelatedItemId}"
                        : "/Feed",
                NotificationType.System or NotificationType.ContentRemoved or NotificationType.Warning or NotificationType.Information => 
                    // System notifications stay on notifications page
                    "/Account/Notifications",
                _ => "/Feed"
            };
        }
    }
}