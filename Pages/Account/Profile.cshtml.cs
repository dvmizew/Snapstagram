using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public ProfileModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context, NotificationService notificationService)
        {
            _userManager = userManager;
            _context = context;
            _notificationService = notificationService;
        }

        public ApplicationUser CurrentUser { get; set; } = default!;
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Album> Albums { get; set; } = new List<Album>();
        public bool IsOwnProfile { get; set; }
        public bool CanViewProfile { get; set; }
        public string? StatusMessage { get; set; }
        
        // Friend request related properties
        public FriendRequestStatus? FriendshipStatus { get; set; }
        public FriendRequest? CurrentFriendRequest { get; set; }
        public int FriendsCount { get; set; }
        public int PendingRequestsCount { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        [BindProperty]
        public PostInputModel PostInput { get; set; } = default!;

        [BindProperty]
        public CommentInputModel CommentInput { get; set; } = default!;

        [BindProperty]
        public CreateAlbumInputModel AlbumInput { get; set; } = new CreateAlbumInputModel();

        public class InputModel
        {
            [Display(Name = "Username")]
            [StringLength(50)]
            public string? UserName { get; set; }

            [Display(Name = "First Name")]
            [StringLength(50)]
            public string? FirstName { get; set; }

            [Display(Name = "Last Name")]
            [StringLength(50)]
            public string? LastName { get; set; }

            [Display(Name = "Bio")]
            [StringLength(500)]
            public string? Bio { get; set; }

            [Display(Name = "Email")]
            [EmailAddress]
            public string? Email { get; set; }

            [Display(Name = "Phone Number")]
            [Phone]
            public string? PhoneNumber { get; set; }

            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            [Display(Name = "Location")]
            [StringLength(100)]
            public string? Location { get; set; }

            [Display(Name = "Website")]
            [Url]
            [StringLength(200)]
            public string? Website { get; set; }

            [Display(Name = "Occupation")]
            [StringLength(100)]
            public string? Occupation { get; set; }

            [Display(Name = "Profile Picture")]
            public IFormFile? ProfilePicture { get; set; }

            [Display(Name = "Profile is Public")]
            public bool IsProfilePublic { get; set; }
        }

        public class PostInputModel
        {
            [Display(Name = "What's on your mind?")]
            [StringLength(2000)]
            public string? Content { get; set; }

            [Display(Name = "Photo")]
            public IFormFile? Photo { get; set; }
        }

        public class CommentInputModel
        {
            [Required]
            [StringLength(500)]
            public string Content { get; set; } = string.Empty;
            
            [Required]
            public int PostId { get; set; }
        }

        public class CreateAlbumInputModel
        {
            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [StringLength(500)]
            public string? Description { get; set; }

            [Required]
            [Display(Name = "Photos")]
            public List<IFormFile> Photos { get; set; } = new List<IFormFile>();
        }

        public async Task<IActionResult> OnGetAsync(string? id = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // If no ID provided, show current user's profile
            var targetUserId = id ?? currentUser.Id;
            var targetUser = await _userManager.FindByIdAsync(targetUserId);

            if (targetUser == null)
            {
                return NotFound();
            }

            CurrentUser = targetUser;
            IsOwnProfile = currentUser.Id == targetUser.Id;

            // Load friendship information if viewing another user's profile
            if (!IsOwnProfile)
            {
                await LoadFriendshipStatusAsync(currentUser.Id, targetUser.Id);
            }

            // Check if user can view this profile
            // Users can view their own profile, public profiles, or profiles of accepted friends
            bool areFriends = !IsOwnProfile && FriendshipStatus == FriendRequestStatus.Accepted;
            CanViewProfile = IsOwnProfile || targetUser.IsProfilePublic || areFriends;

            if (CanViewProfile)
            {
                // Load user's posts
                Posts = await _context.Posts
                    .Where(p => p.UserId == targetUser.Id && p.IsActive && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
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
                    .ToListAsync();                // Load user's albums with photos and creator information
                Albums = await _context.Albums
                    .Where(a => a.UserId == targetUser.Id && !a.IsDeleted)
                    .OrderByDescending(a => a.CreatedAt)
                    .Include(a => a.Photos.Where(p => !p.IsDeleted))
                    .Include(a => a.User)
                    .Include(a => a.Photos)
                        .ThenInclude(p => p.Likes)
                    .Include(a => a.Photos)
                        .ThenInclude(p => p.Comments.Where(c => !c.IsDeleted))
                            .ThenInclude(c => c.User)
                    .ToListAsync();
            }

            // Initialize input model for editing (only if own profile)
            if (IsOwnProfile)
            {
                Input = new InputModel
                {
                    UserName = CurrentUser.UserName,
                    FirstName = CurrentUser.FirstName,
                    LastName = CurrentUser.LastName,
                    Bio = CurrentUser.Bio,
                    Email = CurrentUser.Email,
                    PhoneNumber = CurrentUser.PhoneNumber,
                    DateOfBirth = CurrentUser.DateOfBirth,
                    Location = CurrentUser.Location,
                    Website = CurrentUser.Website,
                    Occupation = CurrentUser.Occupation,
                    IsProfilePublic = CurrentUser.IsProfilePublic
                };
                
                // Load pending friend requests count for own profile
                PendingRequestsCount = await _context.FriendRequests
                    .CountAsync(fr => fr.ReceiverId == currentUser.Id && fr.Status == FriendRequestStatus.Pending);
            }
            
            // Load friends count
            FriendsCount = await _context.FriendRequests
                .CountAsync(fr => 
                    (fr.SenderId == targetUser.Id || fr.ReceiverId == targetUser.Id) && 
                    fr.Status == FriendRequestStatus.Accepted);

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Clear ModelState errors for non-Input fields to avoid validation conflicts
            // This is necessary because the page has multiple forms and input models
            var keysToRemove = ModelState.Keys.Where(k => !k.StartsWith("Input.")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            if (!ModelState.IsValid)
            {
                CurrentUser = user;
                IsOwnProfile = true;
                CanViewProfile = true;
                await LoadPostsAsync(user.Id);
                return Page();
            }

            // Handle profile picture upload if provided
            if (Input.ProfilePicture != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(Input.ProfilePicture.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("Input.ProfilePicture", "Please upload a valid image file (JPG, PNG, or GIF).");
                    CurrentUser = user;
                    IsOwnProfile = true;
                    CanViewProfile = true;
                    await LoadPostsAsync(user.Id);
                    return Page();
                }
                
                if (Input.ProfilePicture.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError("Input.ProfilePicture", "Profile picture size must be less than 5MB.");
                    CurrentUser = user;
                    IsOwnProfile = true;
                    CanViewProfile = true;
                    await LoadPostsAsync(user.Id);
                    return Page();
                }

                try
                {
                    var uploadsFolder = Path.Combine("wwwroot", "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.ProfilePicture.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.ProfilePicture.CopyToAsync(fileStream);
                    }

                    // Delete old profile picture if it exists
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    {
                        var oldImagePath = Path.Combine("wwwroot", user.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    user.ProfilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
                }
                catch (Exception)
                {
                    ModelState.AddModelError("Input.ProfilePicture", "Failed to upload profile picture. Please try again.");
                    CurrentUser = user;
                    IsOwnProfile = true;
                    CanViewProfile = true;
                    await LoadPostsAsync(user.Id);
                    return Page();
                }
            }

            // Check if username is changing and validate it
            if (!string.IsNullOrWhiteSpace(Input.UserName) && !string.Equals(user.UserName, Input.UserName, StringComparison.OrdinalIgnoreCase))
            {
                // Check if username is already taken
                var existingUser = await _userManager.FindByNameAsync(Input.UserName);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError("Input.UserName", "This username is already taken. Please choose a different one.");
                    CurrentUser = user;
                    IsOwnProfile = true;
                    CanViewProfile = true;
                    await LoadPostsAsync(user.Id);
                    return Page();
                }

                // Update the username
                var setUserNameResult = await _userManager.SetUserNameAsync(user, Input.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    foreach (var error in setUserNameResult.Errors)
                    {
                        ModelState.AddModelError("Input.UserName", error.Description);
                    }
                    CurrentUser = user;
                    IsOwnProfile = true;
                    CanViewProfile = true;
                    await LoadPostsAsync(user.Id);
                    return Page();
                }
            }

            // Update user properties
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Bio = Input.Bio;
            user.Email = Input.Email;
            user.PhoneNumber = Input.PhoneNumber;
            user.DateOfBirth = Input.DateOfBirth;
            user.Location = Input.Location;
            user.Website = Input.Website;
            user.Occupation = Input.Occupation;
            user.IsProfilePublic = Input.IsProfilePublic;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                StatusMessage = "Your profile has been updated successfully.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            CurrentUser = user;
            IsOwnProfile = true;
            CanViewProfile = true;
            await LoadPostsAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostCreatePostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Clear ModelState errors for non-PostInput fields to avoid validation conflicts
            // This is necessary because the page has multiple forms and input models
            var keysToRemove = ModelState.Keys.Where(k => !k.StartsWith("PostInput.")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            // Allow flexible posting: content only, photo only, or both
            // No validation required - users can post just an image, just text, or both

            // Validate photo file if provided
            if (PostInput.Photo != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(PostInput.Photo.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PostInput.Photo", "Please upload a valid image file (JPG, PNG, or GIF).");
                }
                
                if (PostInput.Photo.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError("PostInput.Photo", "Photo size must be less than 5MB.");
                }
            }

            if (!ModelState.IsValid)
            {
                CurrentUser = user;
                IsOwnProfile = true;
                CanViewProfile = true;
                await LoadPostsAsync(user.Id);
                return Page();
            }

            string? imageUrl = null;

            // Handle photo upload if provided
            if (PostInput.Photo != null)
            {
                try
                {
                    var uploadsFolder = Path.Combine("wwwroot", "uploads", "posts");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(PostInput.Photo.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await PostInput.Photo.CopyToAsync(fileStream);
                    }

                    imageUrl = $"/uploads/posts/{uniqueFileName}";
                }
                catch (Exception)
                {
                    ModelState.AddModelError("PostInput.Photo", "Failed to upload photo. Please try again.");
                    CurrentUser = user;
                    IsOwnProfile = true;
                    CanViewProfile = true;
                    await LoadPostsAsync(user.Id);
                    return Page();
                }
            }

            var post = new Post
            {
                UserId = user.Id,
                Caption = PostInput.Content ?? string.Empty,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            StatusMessage = "Your post has been created successfully.";
            return RedirectToPage();
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

        public async Task<IActionResult> OnPostAddCommentAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Manual validation for comment content
            if (string.IsNullOrWhiteSpace(CommentInput.Content) || CommentInput.Content.Length > 500)
            {
                return new JsonResult(new { success = false, message = "Comment must not be empty and must be less than 500 characters." });
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

        public async Task<IActionResult> OnPostDeletePostAsync(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                return new JsonResult(new { success = false, message = "Post not found" });
            }

            post.IsDeleted = true;
            post.DeletedAt = DateTime.UtcNow;
            post.DeletedByUserId = user.Id;
            
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostEditPostAsync(int postId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.UserId == user.Id);
            if (post == null)
            {
                return new JsonResult(new { success = false, message = "Post not found or you don't have permission to edit it" });
            }

            // Allow flexible editing: content can be empty, image can be removed, or both can exist
            // Users can edit to have just text, just image, or both
            post.Caption = content ?? string.Empty;
            post.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return new JsonResult(new { 
                success = true, 
                post = new {
                    id = post.Id,
                    caption = post.Caption,
                    updatedAt = post.UpdatedAt?.ToString("MMM dd, yyyy 'at' h:mm tt")
                }
            });
        }

        public async Task<IActionResult> OnPostRemoveProfilePictureAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            // Delete the profile picture file if it exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                var imagePath = Path.Combine("wwwroot", user.ProfilePictureUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                user.ProfilePictureUrl = null;
                await _userManager.UpdateAsync(user);
            }

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int commentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var comment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                return new JsonResult(new { success = false, message = "Comment not found" });
            }

            // Check if user owns the comment or the post
            if (comment.UserId != user.Id && comment.Post?.UserId != user.Id)
            {
                return new JsonResult(new { success = false, message = "You don't have permission to delete this comment" });
            }

            comment.IsDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;
            comment.DeletedByUserId = user.Id;
            comment.DeleteReason = "Deleted by user";

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
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

        public async Task<IActionResult> OnPostAddReplyAsync(int commentId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(content) || content.Length > 500)
            {
                return new JsonResult(new { success = false, message = "Reply content is invalid" });
            }

            var parentComment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (parentComment == null || parentComment.IsDeleted)
            {
                return new JsonResult(new { success = false, message = "Comment not found" });
            }

            var reply = new CommentReply
            {
                CommentId = commentId,
                UserId = user.Id,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommentReplies.Add(reply);
            await _context.SaveChangesAsync();
            
            // Send notification to comment author (if not replying to own comment)
            if (parentComment.UserId != user.Id && !string.IsNullOrEmpty(parentComment.UserId))
            {
                try
                {
                    await _notificationService.SendNotificationAsync(
                        parentComment.UserId,
                        $"{user.FirstName} {user.LastName} replied to your comment",
                        NotificationType.Comment,
                        reply.Id.ToString()
                    );
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the reply operation
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
            }

            // Load the reply with user data
            var replyWithUser = await _context.CommentReplies
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == reply.Id);

            if (replyWithUser == null)
            {
                return new JsonResult(new { success = false, message = "Reply not found" });
            }

            return new JsonResult(new 
            { 
                success = true, 
                reply = new 
                {
                    id = replyWithUser.Id,
                    content = replyWithUser.Content,
                    createdAt = replyWithUser.CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt"),
                    user = new 
                    {
                        firstName = replyWithUser.User?.FirstName,
                        lastName = replyWithUser.User?.LastName,
                        profilePictureUrl = replyWithUser.User?.ProfilePictureUrl
                    }
                }
            });
        }

        public async Task<IActionResult> OnPostDeleteReplyAsync(int replyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var reply = await _context.CommentReplies
                .Include(r => r.Comment)
                .ThenInclude(c => c!.Post)
                .FirstOrDefaultAsync(r => r.Id == replyId);

            if (reply == null)
            {
                return new JsonResult(new { success = false, message = "Reply not found" });
            }

            // Check if user owns the reply, comment, or post
            if (reply.UserId != user.Id && 
                reply.Comment?.UserId != user.Id && 
                reply.Comment?.Post?.UserId != user.Id)
            {
                return new JsonResult(new { success = false, message = "You don't have permission to delete this reply" });
            }

            reply.IsDeleted = true;
            reply.DeletedAt = DateTime.UtcNow;
            reply.DeletedByUserId = user.Id;

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostEditCommentAsync(int commentId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return new JsonResult(new { success = false, message = "Comment content cannot be empty" });
            }

            if (content.Length > 2000)
            {
                return new JsonResult(new { success = false, message = "Comment is too long (max 2000 characters)" });
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted);

            if (comment == null)
            {
                return new JsonResult(new { success = false, message = "Comment not found" });
            }

            // Check if user owns the comment
            if (comment.UserId != user.Id)
            {
                return new JsonResult(new { success = false, message = "You can only edit your own comments" });
            }

            // Update the comment
            comment.Content = content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        private async Task LoadPostsAsync(string userId)
        {
            Posts = await _context.Posts
                .Where(p => p.UserId == userId && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
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
                .ToListAsync();
        }

        // Friend Request Methods
        public async Task<IActionResult> OnPostSendFriendRequestAsync(string receiverId, string? message = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            if (user.Id == receiverId)
            {
                return new JsonResult(new { success = false, message = "You cannot send a friend request to yourself" });
            }

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null)
            {
                return new JsonResult(new { success = false, message = "User not found" });
            }

            // Check if a friend request already exists between these users
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    (fr.SenderId == user.Id && fr.ReceiverId == receiverId) ||
                    (fr.SenderId == receiverId && fr.ReceiverId == user.Id));

            if (existingRequest != null)
            {
                if (existingRequest.Status == FriendRequestStatus.Pending)
                {
                    return new JsonResult(new { success = false, message = "A friend request is already pending" });
                }
                if (existingRequest.Status == FriendRequestStatus.Accepted)
                {
                    return new JsonResult(new { success = false, message = "You are already friends" });
                }
            }

            // Create new friend request
            var friendRequest = new FriendRequest
            {
                SenderId = user.Id,
                ReceiverId = receiverId,
                Message = message?.Trim(),
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            // Send notification
            try
            {
                await _notificationService.SendNotificationAsync(
                    receiverId,
                    $"{user.FirstName} {user.LastName} sent you a friend request",
                    NotificationType.FriendRequest,
                    friendRequest.Id.ToString()
                );
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                Console.WriteLine($"Failed to send notification: {ex.Message}");
            }

            return new JsonResult(new { success = true, message = "Friend request sent successfully" });
        }

        public async Task<IActionResult> OnPostRespondToFriendRequestAsync(int requestId, bool accept)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var friendRequest = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == user.Id);

            if (friendRequest == null)
            {
                return new JsonResult(new { success = false, message = "Friend request not found" });
            }

            if (friendRequest.Status != FriendRequestStatus.Pending)
            {
                return new JsonResult(new { success = false, message = "Friend request has already been responded to" });
            }

            friendRequest.Status = accept ? FriendRequestStatus.Accepted : FriendRequestStatus.Rejected;
            friendRequest.RespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification if accepted
            if (accept && friendRequest.SenderId != null)
            {
                try
                {
                    await _notificationService.SendNotificationAsync(
                        friendRequest.SenderId,
                        $"{user.FirstName} {user.LastName} accepted your friend request",
                        NotificationType.FriendRequestAccepted,
                        friendRequest.Id.ToString()
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send notification: {ex.Message}");
                }
            }

            var message = accept ? "Friend request accepted" : "Friend request rejected";
            return new JsonResult(new { success = true, message = message });
        }

        public async Task<IActionResult> OnPostCancelFriendRequestAsync(int requestId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.SenderId == user.Id);

            if (friendRequest == null)
            {
                return new JsonResult(new { success = false, message = "Friend request not found" });
            }

            if (friendRequest.Status != FriendRequestStatus.Pending)
            {
                return new JsonResult(new { success = false, message = "Can only cancel pending friend requests" });
            }

            friendRequest.Status = FriendRequestStatus.Cancelled;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Friend request cancelled" });
        }

        public async Task<IActionResult> OnPostRemoveFriendAsync(string friendId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var friendship = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    ((fr.SenderId == user.Id && fr.ReceiverId == friendId) ||
                     (fr.SenderId == friendId && fr.ReceiverId == user.Id)) &&
                    fr.Status == FriendRequestStatus.Accepted);

            if (friendship == null)
            {
                return new JsonResult(new { success = false, message = "Friendship not found" });
            }

            _context.FriendRequests.Remove(friendship);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true, message = "Friend removed successfully" });
        }
        
        private async Task LoadFriendshipStatusAsync(string currentUserId, string targetUserId)
        {
            CurrentFriendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    (fr.SenderId == currentUserId && fr.ReceiverId == targetUserId) ||
                    (fr.SenderId == targetUserId && fr.ReceiverId == currentUserId));
                    
            if (CurrentFriendRequest != null)
            {
                FriendshipStatus = CurrentFriendRequest.Status;
            }
        }

        public async Task<IActionResult> OnPostCreateAlbumAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Clear ModelState errors for non-AlbumInput fields
            var keysToRemove = ModelState.Keys.Where(k => !k.StartsWith("AlbumInput.")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }

            // Validate photos
            if (AlbumInput.Photos.Count == 0)
            {
                ModelState.AddModelError("AlbumInput.Photos", "Please select at least one photo for the album.");
            }
            else
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                foreach (var photo in AlbumInput.Photos)
                {
                    var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("AlbumInput.Photos", $"File '{photo.FileName}' is not a valid image. Please upload only JPG, PNG, or GIF files.");
                    }
                    if (photo.Length > 5 * 1024 * 1024) // 5MB limit
                    {
                        ModelState.AddModelError("AlbumInput.Photos", $"File '{photo.FileName}' exceeds the 5MB size limit.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                CurrentUser = user;
                IsOwnProfile = true;
                CanViewProfile = true;
                await LoadPostsAsync(user.Id);
                return Page();
            }

            // Create album
            var album = new Album
            {
                UserId = user.Id,
                Name = AlbumInput.Name,
                Description = AlbumInput.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Albums.Add(album);
            await _context.SaveChangesAsync(); // Save to get the album ID

            // Process and save photos
            var uploadsFolder = Path.Combine("wwwroot", "uploads", "albums", album.Id.ToString());
            Directory.CreateDirectory(uploadsFolder);

            int orderIndex = 0;
            foreach (var photo in AlbumInput.Photos)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(fileStream);
                }

                var albumPhoto = new AlbumPhoto
                {
                    AlbumId = album.Id,
                    Url = $"/uploads/albums/{album.Id}/{uniqueFileName}",
                    Name = Path.GetFileNameWithoutExtension(photo.FileName),
                    CreatedAt = DateTime.UtcNow,
                    OrderIndex = orderIndex++
                };

                _context.AlbumPhotos.Add(albumPhoto);
            }

            await _context.SaveChangesAsync();

            // Reload posts and albums for the current user so the new album appears
            CurrentUser = user;
            IsOwnProfile = true;
            CanViewProfile = true;
            await LoadPostsAsync(user.Id);
            Albums = await _context.Albums
                .Where(a => a.UserId == user.Id && !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .Include(a => a.Photos.Where(p => !p.IsDeleted))
                .Include(a => a.User)
                .Include(a => a.Photos)
                    .ThenInclude(p => p.Likes)
                .Include(a => a.Photos)
                    .ThenInclude(p => p.Comments.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.User)
                .ToListAsync();

            StatusMessage = "Your album has been created successfully.";
            return RedirectToPage(new { id = user.Id });
        }

        public async Task<IActionResult> OnPostLikeAlbumPhotoAsync(int photoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            var photo = await _context.AlbumPhotos.FindAsync(photoId);
            if (photo == null)
            {
                return new JsonResult(new { success = false, message = "Photo not found" });
            }

            // Check if user already liked this photo
            var existingLike = await _context.AlbumPhotoLikes
                .FirstOrDefaultAsync(l => l.PhotoId == photoId && l.UserId == user.Id);

            bool isLiked;

            if (existingLike != null)
            {
                // Unlike the photo
                _context.AlbumPhotoLikes.Remove(existingLike);
                isLiked = false;
            }
            else
            {
                // Like the photo
                var like = new AlbumPhotoLike
                {
                    PhotoId = photoId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AlbumPhotoLikes.Add(like);
                isLiked = true;
            }

            await _context.SaveChangesAsync();

            // Get updated likes count
            var likesCount = await _context.AlbumPhotoLikes
                .CountAsync(l => l.PhotoId == photoId);

            return new JsonResult(new { 
                success = true, 
                liked = isLiked, 
                likeCount = likesCount
            });
        }

        public async Task<IActionResult> OnPostAddAlbumPhotoCommentAsync(int photoId, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return new JsonResult(new { success = false, message = "Comment cannot be empty" });
            }

            var photo = await _context.AlbumPhotos.FindAsync(photoId);
            if (photo == null)
            {
                return new JsonResult(new { success = false, message = "Photo not found" });
            }

            var comment = new AlbumPhotoComment
            {
                PhotoId = photoId,
                UserId = user.Id,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.AlbumPhotoComments.Add(comment);
            await _context.SaveChangesAsync();

            // Load the comment with user data
            var commentWithUser = await _context.AlbumPhotoComments
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
    }
}
