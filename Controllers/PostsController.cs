using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Controllers;

[ApiController, Route("api/[controller]"), Authorize]
public class PostsController(IPostService postService, UserManager<User> userManager, NotificationService notificationService) : ControllerBase
{
    [HttpGet("{postId}")]
    public async Task<IActionResult> GetPost(int postId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var post = await postService.GetPostByIdAsync(postId);
        if (post == null) return NotFound();

        // Check if user has liked this post
        var isLiked = await postService.HasUserLikedPostAsync(postId, user.Id);

        return Ok(new
        {
            id = post.Id,
            imageUrl = post.ImageUrl,
            videoUrl = post.VideoUrl,
            caption = post.Caption,
            likesCount = post.LikesCount,
            commentsCount = post.CommentsCount,
            createdAt = post.CreatedAt,
            isLiked = isLiked,
            user = new
            {
                id = post.User.Id,
                displayName = post.User.DisplayName,
                userName = post.User.UserName,
                profileImageUrl = post.User.ProfileImageUrl
            }
        });
    }

    [HttpPost("{postId}/like")]
    public async Task<IActionResult> ToggleLike(int postId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var post = await postService.GetPostByIdAsync(postId);
        if (post == null) return NotFound();

        var isLiked = await postService.ToggleLikeAsync(postId, user.Id);
        
        // Create notification if user liked the post (not their own)
        if (isLiked && user.Id != post.UserId)
        {
            await notificationService.CreateLikeNotificationAsync(post.UserId, user.Id, postId);
        }
        
        // Get updated post to return current likes count
        var updatedPost = await postService.GetPostByIdAsync(postId);
        return Ok(new { liked = isLiked, likesCount = updatedPost?.LikesCount ?? 0 });
    }

    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> AddComment(int postId, [FromBody] CommentRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Comment text is required");

        var post = await postService.GetPostByIdAsync(postId);
        if (post == null) return NotFound();

        var comment = await postService.AddCommentAsync(postId, user.Id, request.Text);
        
        // Create notification if user commented on someone else's post
        if (user.Id != post.UserId)
        {
            await notificationService.CreateCommentNotificationAsync(post.UserId, user.Id, postId, comment.Id);
        }
        
        return Ok(new
        {
            id = comment.Id,
            text = comment.Text,
            createdAt = comment.CreatedAt,
            user = new { displayName = user.DisplayName, profileImageUrl = user.ProfileImageUrl }
        });
    }

    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetComments(int postId)
    {
        var post = await postService.GetPostByIdAsync(postId);
        if (post == null) return NotFound();

        var comments = post.Comments.OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                id = c.Id,
                text = c.Text,
                createdAt = c.CreatedAt,
                user = new { displayName = c.User.DisplayName, profileImageUrl = c.User.ProfileImageUrl }
            });

        return Ok(comments);
    }

    [HttpPost("{postId}/bookmark")]
    public async Task<IActionResult> ToggleBookmark(int postId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var post = await postService.GetPostByIdAsync(postId);
        if (post == null) return NotFound();

        // Implementation would depend on if you have a bookmark feature
        // For now, return a placeholder response
        return Ok(new { bookmarked = true });
    }
}

public class CommentRequest
{
    public string Text { get; set; } = string.Empty;
}
