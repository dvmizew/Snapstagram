using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Controllers;

[ApiController, Route("api/[controller]"), Authorize]
public class UsersController(ApplicationDbContext context, UserManager<User> userManager, NotificationService notificationService) : ControllerBase
{
    [HttpGet("suggested")]
    public async Task<IActionResult> GetSuggestedUsers([FromQuery] int count = 5)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        // Get users that the current user is not following
        var followingIds = await context.Follows
            .Where(f => f.FollowerId == currentUser.Id)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(currentUser.Id); // Don't suggest self

        var suggestedUsers = await context.Users
            .Where(u => !followingIds.Contains(u.Id))
            .OrderBy(u => Guid.NewGuid()) // Random order
            .Take(count)
            .Select(u => new
            {
                id = u.Id,
                userName = u.UserName,
                displayName = u.DisplayName,
                profileImageUrl = u.ProfileImageUrl,
                mutualFriendsCount = context.Follows
                    .Where(f => f.FollowerId == u.Id && followingIds.Contains(f.FollowingId))
                    .Count()
            })
            .ToListAsync();

        return Ok(suggestedUsers);
    }

    [HttpPost("{userId}/follow")]
    public async Task<IActionResult> ToggleFollow(string userId)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        if (currentUser.Id == userId)
            return BadRequest(new { message = "Cannot follow yourself" });

        var targetUser = await context.Users.FindAsync(userId);
        if (targetUser == null) return NotFound();

        var existingFollow = await context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == userId);

        bool isFollowing;
        
        if (existingFollow != null)
        {
            context.Follows.Remove(existingFollow);
            isFollowing = false;
        }
        else
        {
            context.Follows.Add(new Follow 
            { 
                FollowerId = currentUser.Id, 
                FollowingId = userId 
            });
            isFollowing = true;
            
            // Create notification for the followed user
            await notificationService.CreateFollowNotificationAsync(userId, currentUser.Id);
        }

        await context.SaveChangesAsync();

        return Ok(new { isFollowing = isFollowing });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Ok(new List<object>());

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var users = await context.Users
            .Where(u => u.Id != currentUser.Id && 
                       (u.UserName!.Contains(query) || 
                        u.DisplayName!.Contains(query)))
            .Take(limit)
            .Select(u => new
            {
                id = u.Id,
                userName = u.UserName,
                displayName = u.DisplayName,
                profileImageUrl = u.ProfileImageUrl,
                isFollowing = context.Follows
                    .Any(f => f.FollowerId == currentUser.Id && f.FollowingId == u.Id)
            })
            .ToListAsync();

        return Ok(users);
    }
}
