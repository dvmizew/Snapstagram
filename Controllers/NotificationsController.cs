using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Controllers;

[ApiController, Route("api/[controller]"), Authorize]
public class NotificationsController(NotificationService notificationService, UserManager<User> userManager) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 0, [FromQuery] int pageSize = 20)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var notifications = await notificationService.GetNotificationsAsync(user.Id, page, pageSize);
        
        var result = notifications.Select(n => new
        {
            id = n.Id,
            type = n.Type.ToString(),
            message = n.Message,
            isRead = n.IsRead,
            createdAt = n.CreatedAt,
            actor = new
            {
                id = n.Actor?.Id,
                userName = n.Actor?.UserName,
                displayName = n.Actor?.DisplayName,
                profileImageUrl = n.Actor?.ProfileImageUrl
            },
            post = n.Post != null ? new
            {
                id = n.Post.Id,
                imageUrl = n.Post.ImageUrl
            } : null
        });

        return Ok(result);
    }

    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await notificationService.MarkAsReadAsync(user.Id, notificationId);
        return Ok();
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        await notificationService.MarkAllAsReadAsync(user.Id);
        return Ok();
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var count = await notificationService.GetUnreadCountAsync(user.Id);
        return Ok(new { count });
    }
}