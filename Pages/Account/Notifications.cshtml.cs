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
    }
}