using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IProfileService _profileService;

    public ProfileController(UserManager<User> userManager, IProfileService profileService)
    {
        _userManager = userManager;
        _profileService = profileService;
    }

    [HttpGet("followers/{userId}")]
    public async Task<IActionResult> GetFollowers(string userId, int page = 0, int pageSize = 20)
    {
        var followers = await _profileService.GetFollowersAsync(userId, page, pageSize);
        var result = followers.Select(u => new
        {
            id = u.Id,
            userName = u.UserName,
            displayName = u.DisplayName,
            profileImageUrl = u.ProfileImageUrl,
            isPrivate = u.IsPrivate
        });

        return Ok(result);
    }

    [HttpGet("following/{userId}")]
    public async Task<IActionResult> GetFollowing(string userId, int page = 0, int pageSize = 20)
    {
        var following = await _profileService.GetFollowingAsync(userId, page, pageSize);
        var result = following.Select(u => new
        {
            id = u.Id,
            userName = u.UserName,
            displayName = u.DisplayName,
            profileImageUrl = u.ProfileImageUrl,
            isPrivate = u.IsPrivate
        });

        return Ok(result);
    }

    [HttpGet("posts/{userId}")]
    public async Task<IActionResult> GetUserPosts(string userId, int page = 0, int pageSize = 12)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Check if user can view posts
        var targetUser = await _profileService.GetUserProfileAsync(userId);
        if (targetUser == null)
        {
            return NotFound();
        }

        // Check permissions
        var isOwnProfile = currentUser.Id == userId;
        var isFollowing = await _profileService.IsFollowingAsync(currentUser.Id, userId);
        var canViewPosts = isOwnProfile || !targetUser.IsPrivate || isFollowing;

        if (!canViewPosts)
        {
            return Forbid();
        }

        var posts = await _profileService.GetUserPostsAsync(userId, page, pageSize);
        var result = posts.Select(p => new
        {
            id = p.Id,
            imageUrl = p.ImageUrl,
            videoUrl = p.VideoUrl,
            caption = p.Caption,
            likesCount = p.LikesCount,
            commentsCount = p.CommentsCount,
            createdAt = p.CreatedAt
        });

        return Ok(result);
    }

    [HttpPost("follow/{userId}")]
    public async Task<IActionResult> ToggleFollow(string userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (currentUser.Id == userId)
        {
            return BadRequest("Cannot follow yourself");
        }

        var isNowFollowing = await _profileService.ToggleFollowAsync(currentUser.Id, userId);
        
        return Ok(new { isFollowing = isNowFollowing });
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers(string query, int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(new List<object>());
        }

        var users = await _profileService.SearchUsersAsync(query, limit);
        var result = users.Select(u => new
        {
            id = u.Id,
            userName = u.UserName,
            displayName = u.DisplayName,
            profileImageUrl = u.ProfileImageUrl,
            isPrivate = u.IsPrivate
        });

        return Ok(result);
    }

    [HttpGet("stats/{userId}")]
    public async Task<IActionResult> GetUserStats(string userId)
    {
        var postsCount = await _profileService.GetPostsCountAsync(userId);
        var followersCount = await _profileService.GetFollowersCountAsync(userId);
        var followingCount = await _profileService.GetFollowingCountAsync(userId);

        return Ok(new
        {
            postsCount,
            followersCount,
            followingCount
        });
    }
}
