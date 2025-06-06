using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.ViewComponents
{
    public class NotificationCountViewComponent : ViewComponent
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationCountViewComponent(NotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return View(0);
            }

            int count = await _notificationService.GetUnreadNotificationCountAsync(user.Id);
            return View(count);
        }
    }
}