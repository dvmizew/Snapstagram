using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Models;
using Snapstagram.Services;
using System.Security.Claims;

namespace Snapstagram.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(NotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet("GetRecent")]
        public async Task<IActionResult> GetRecent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                var notifications = await _notificationService.GetUserNotificationsAsync(userId);
                var recentNotifications = notifications.Take(10).Select(n => new
                {
                    id = n.Id,
                    message = n.Message,
                    senderName = n.SenderName,
                    type = n.Type.ToString(),
                    createdAt = n.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    isRead = n.IsRead,
                    relatedItemId = n.RelatedItemId,
                    timeAgo = GetTimeAgo(n.CreatedAt)
                }).ToList();

                return Json(new { success = true, notifications = recentNotifications });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to retrieve notifications" });
            }
        }

        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead([FromForm] int notificationId)
        {
            try
            {
                await _notificationService.MarkNotificationAsReadAsync(notificationId);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to mark notification as read" });
            }
        }

        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to mark all notifications as read" });
            }
        }

        [HttpGet("GetCount")]
        public async Task<IActionResult> GetCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
                return Json(new { success = true, count = count });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to get notification count" });
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> DeleteNotification([FromForm] int notificationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                await _notificationService.DeleteNotificationAsync(notificationId, userId);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to delete notification" });
            }
        }

        [HttpPost("DeleteAll")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                await _notificationService.DeleteAllNotificationsAsync(userId);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Failed to delete all notifications" });
            }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w ago";
            
            return dateTime.ToString("MMM dd");
        }
    }
}
