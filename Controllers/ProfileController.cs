using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Models;
using Snapstagram.Services;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IProfileService _profileService;
    private readonly NotificationService _notificationService;

    public ProfileController(UserManager<User> userManager, IProfileService profileService, NotificationService notificationService)
    {
        _userManager = userManager;
        _profileService = profileService;
        _notificationService = notificationService;
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
        
        // Create notification if user started following someone
        if (isNowFollowing)
        {
            await _notificationService.CreateFollowNotificationAsync(userId, currentUser.Id);
        }
        
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

    [HttpPost("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _profileService.UpdateProfileAsync(
                currentUser.Id, 
                request.DisplayName, 
                request.Bio, 
                request.IsPrivate
            );

            if (result)
            {
                return Ok(new { message = "Profile updated successfully" });
            }

            return BadRequest(new { message = "Failed to update profile" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating profile" });
        }
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _userManager.ChangePasswordAsync(currentUser, request.CurrentPassword, request.NewPassword);
            
            if (result.Succeeded)
            {
                return Ok(new { message = "Password changed successfully" });
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new { message = "Failed to change password", errors });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    [HttpPost("update-image")]
    public async Task<IActionResult> UpdateProfileImage([FromBody] UpdateProfileImageRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _profileService.UpdateProfileImageAsync(currentUser.Id, request.ProfileImageUrl);
            
            if (result)
            {
                return Ok(new { message = "Profile image updated successfully" });
            }

            return BadRequest(new { message = "Failed to update profile image" });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while updating profile image" });
        }
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (profileImage == null || profileImage.Length == 0)
        {
            return BadRequest(new { message = "Please select an image file." });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(profileImage.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Please upload a valid image file (JPEG, PNG, or GIF)." });
        }

        // Validate file size (5MB max)
        if (profileImage.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "Image file must be smaller than 5MB." });
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
            Directory.CreateDirectory(uploadsPath);

            // Delete old profile image if it exists and is not the default
            if (!string.IsNullOrEmpty(currentUser.ProfileImageUrl) && 
                !currentUser.ProfileImageUrl.Contains("default-avatar"))
            {
                var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", currentUser.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(profileImage.FileName);
            var fileName = $"{currentUser.Id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            // Update user's profile image URL
            var imageUrl = $"/uploads/profiles/{fileName}";
            var success = await _profileService.UpdateProfileImageAsync(currentUser.Id, imageUrl);

            if (success)
            {
                return Ok(new { 
                    message = "Profile image updated successfully!", 
                    imageUrl = imageUrl 
                });
            }
            else
            {
                return BadRequest(new { message = "Failed to update profile image." });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while uploading the image.", error = ex.Message });
        }
    }
}

public class UpdateProfileRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(500)]
    public string Bio { get; set; } = string.Empty;

    public bool IsPrivate { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UpdateProfileImageRequest
{
    [Required]
    [Url]
    public string ProfileImageUrl { get; set; } = string.Empty;
}
