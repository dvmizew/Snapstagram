using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Models;
using Snapstagram.Services;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Controllers;

[ApiController, Route("api/[controller]"), Authorize]
public class StoriesController(IStoryService storyService, UserManager<User> userManager) : ControllerBase
{
    [HttpGet("{storyId}")]
    public async Task<IActionResult> GetStory(int storyId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Get all active stories to find the requested one
        var allStories = await storyService.GetActiveStoriesAsync(user.Id);
        var story = allStories.FirstOrDefault(s => s.Id == storyId);
        
        if (story == null) return NotFound();

        return Ok(new
        {
            id = story.Id,
            mediaUrl = story.MediaUrl,
            mediaType = story.MediaType,
            text = story.Text,
            createdAt = story.CreatedAt,
            expiresAt = story.ExpiresAt,
            viewsCount = story.ViewsCount,
            user = new
            {
                id = story.User.Id,
                displayName = story.User.DisplayName,
                userName = story.User.UserName,
                profileImageUrl = story.User.ProfileImageUrl
            }
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStory([FromBody] CreateStoryRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var story = await storyService.CreateStoryAsync(user.Id, request.MediaUrl, request.MediaType, request.Text ?? "");
            
            return Ok(new 
            { 
                message = "Story created successfully",
                storyId = story.Id
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while creating the story" });
        }
    }

    [HttpPost("{storyId}/view")]
    public async Task<IActionResult> ViewStory(int storyId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var viewed = await storyService.ViewStoryAsync(storyId, user.Id);
        
        return Ok(new { viewed = viewed });
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserStories(string userId)
    {
        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var stories = await storyService.GetUserStoriesAsync(userId);
        
        var storiesData = stories.Select(s => new
        {
            id = s.Id,
            mediaUrl = s.MediaUrl,
            mediaType = s.MediaType,
            text = s.Text,
            createdAt = s.CreatedAt,
            expiresAt = s.ExpiresAt,
            viewsCount = s.ViewsCount,
            isViewed = s.Views.Any(v => v.UserId == currentUser.Id)
        });

        return Ok(storiesData);
    }
}

public class CreateStoryRequest
{
    [Required]
    [Url]
    public string MediaUrl { get; set; } = string.Empty;

    [Required]
    public string MediaType { get; set; } = "image"; // "image" or "video"

    [StringLength(500)]
    public string? Text { get; set; }
}
