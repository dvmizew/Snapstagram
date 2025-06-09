using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Pages
{
    [Authorize]
    public class FeedModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly ContentModerationService _moderationService;

        public FeedModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context, NotificationService notificationService, ContentModerationService moderationService)
        {
            _userManager = userManager;
            _context = context;
            _notificationService = notificationService;
            _moderationService = moderationService;
        }

        public List<Post> Posts { get; set; } = new List<Post>();
        public ApplicationUser CurrentUser { get; set; } = default!;
        public string? StatusMessage { get; set; }
        public bool IsAdministrator { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            CurrentUser = user;
            IsAdministrator = await _userManager.IsInRoleAsync(user, "Administrator");

            // Get friend IDs where the friendship is accepted
            var friendIds = await _context.FriendRequests
                .Where(fr => fr.Status == FriendRequestStatus.Accepted &&
                           (fr.SenderId == user.Id || fr.ReceiverId == user.Id))
                .Select(fr => fr.SenderId == user.Id ? fr.ReceiverId : fr.SenderId)
                .ToListAsync();

            // Add the current user's ID to see their own posts
            var userIds = new List<string> { user.Id };
            userIds.AddRange(friendIds);

            // Get posts from the user and their friends, or from public profiles
            Posts = await _context.Posts
                .Where(p => p.IsActive && !p.IsDeleted && 
                          (userIds.Contains(p.UserId) || p.User.IsProfilePublic))
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.User)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.CommentLikes)
                        .ThenInclude(cl => cl.User)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.CommentReplies.Where(cr => !cr.IsDeleted))
                        .ThenInclude(cr => cr.User)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .Take(50) // Limit to 50 most recent posts
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostLikePostAsync(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return new JsonResult(new { success = false, message = "Post not found" });
            }

            // Check if user already liked this post
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == user.Id);

            bool isLiked;

            if (existingLike != null)
            {
                // Unlike the post
                _context.Likes.Remove(existingLike);
                isLiked = false;
            }
            else
            {
                // Like the post
                var like = new Like
                {
                    PostId = postId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(like);
                isLiked = true;
            }

            await _context.SaveChangesAsync();

            // Get updated likes with user information
            var updatedLikes = await _context.Likes
                .Where(l => l.PostId == postId)
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new {
                    id = l.Id,
                    userId = l.UserId,
                    createdAt = l.CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt"),
                    user = new {
                        firstName = l.User!.FirstName,
                        lastName = l.User!.LastName,
                        profilePictureUrl = l.User!.ProfilePictureUrl
                    }
                })
                .ToListAsync();

            return new JsonResult(new { 
                success = true, 
                liked = isLiked, 
                likeCount = updatedLikes.Count,
                likes = updatedLikes
            });
        }

        [BindProperty]
        public CommentInputModel CommentInput { get; set; } = new();

        public class CommentInputModel
        {
            [Required]
            public int PostId { get; set; }
            
            [Required]
            [StringLength(500)]
            public string Content { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAddCommentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid comment" });
            }

            var post = await _context.Posts.FindAsync(CommentInput.PostId);
            if (post == null)
            {
                return new JsonResult(new { success = false, message = "Post not found" });
            }

            var comment = new Comment
            {
                PostId = CommentInput.PostId,
                UserId = user.Id,
                Content = CommentInput.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Load the comment with user data
            var commentWithUser = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == comment.Id);

            if (commentWithUser == null)
            {
                return new JsonResult(new { success = false, message = "Comment not found" });
            }

            return new JsonResult(new 
            { 
                success = true, 
                comment = new 
                {
                    id = commentWithUser.Id,
                    content = commentWithUser.Content,
                    createdAt = commentWithUser.CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt"),
                    user = new 
                    {
                        firstName = commentWithUser.User?.FirstName,
                        lastName = commentWithUser.User?.LastName,
                        profilePictureUrl = commentWithUser.User?.ProfilePictureUrl
                    }
                }
            });
        }

        public async Task<IActionResult> OnPostLikeCommentAsync(int commentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null || comment.IsDeleted)
            {
                return new JsonResult(new { success = false, message = "Comment not found" });
            }

            var existingLike = await _context.CommentLikes
                .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == user.Id);

            bool isLiked;
            int likeCount;

            if (existingLike != null)
            {
                _context.CommentLikes.Remove(existingLike);
                isLiked = false;
            }
            else
            {
                var commentLike = new CommentLike
                {
                    CommentId = commentId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CommentLikes.Add(commentLike);
                isLiked = true;
                
                // Send notification to comment author (if not liking own comment)
                if (comment.UserId != user.Id && !string.IsNullOrEmpty(comment.UserId))
                {
                    try
                    {
                        await _notificationService.SendNotificationAsync(
                            comment.UserId,
                            $"{user.FirstName} {user.LastName} liked your comment",
                            NotificationType.Like,
                            comment.Id.ToString()
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't fail the like operation
                        Console.WriteLine($"Failed to send notification: {ex.Message}");
                    }
                }
            }

            await _context.SaveChangesAsync();

            likeCount = await _context.CommentLikes.CountAsync(cl => cl.CommentId == commentId);

            return new JsonResult(new 
            { 
                success = true, 
                liked = isLiked, 
                likeCount = likeCount
            });
        }

        public async Task<IActionResult> OnPostRemovePostAsync(int postId, string reason)
        {
            if (!User.IsInRole("Administrator"))
            {
                return new JsonResult(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }

                await _moderationService.RemovePostAsync(postId, currentUser.Id, reason);
                return new JsonResult(new { success = true, message = "Post removed successfully" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error removing post: {ex.Message}" });
            }
        }

        public async Task<IActionResult> OnPostRemoveCommentAsync(int commentId, string reason)
        {
            if (!User.IsInRole("Administrator"))
            {
                return new JsonResult(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return new JsonResult(new { success = false, message = "User not authenticated" });
                }

                await _moderationService.RemoveCommentAsync(commentId, currentUser.Id, reason);
                return new JsonResult(new { success = true, message = "Comment removed successfully" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Error removing comment: {ex.Message}" });
            }
        }
    }
}
