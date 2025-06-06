using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class ContentModerationModel : PageModel
    {
        private readonly ContentModerationService _moderationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContentModerationModel(
            ContentModerationService moderationService,
            UserManager<ApplicationUser> userManager)
        {
            _moderationService = moderationService;
            _userManager = userManager;
        }

        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Message> Messages { get; set; } = new List<Message>();
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; } = false;

        public async Task OnGetAsync()
        {
            await LoadContentAsync();
        }

        public async Task<IActionResult> OnPostRemovePostAsync(int postId, string reason)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                await _moderationService.RemovePostAsync(postId, currentUser.Id, reason);
                
                StatusMessage = "Post removed successfully and user notified.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing post: {ex.Message}";
                IsSuccess = false;
            }

            await LoadContentAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveCommentAsync(int commentId, string reason)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                await _moderationService.RemoveCommentAsync(commentId, currentUser.Id, reason);
                
                StatusMessage = "Comment removed successfully and user notified.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing comment: {ex.Message}";
                IsSuccess = false;
            }

            await LoadContentAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveMessageAsync(int messageId, string reason)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                await _moderationService.RemoveMessageAsync(messageId, currentUser.Id, reason);
                
                StatusMessage = "Message removed successfully and user notified.";
                IsSuccess = true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error removing message: {ex.Message}";
                IsSuccess = false;
            }

            await LoadContentAsync();
            return Page();
        }

        private async Task LoadContentAsync()
        {
            Posts = await _moderationService.GetReportedPostsAsync();
            Comments = await _moderationService.GetReportedCommentsAsync();
            Messages = await _moderationService.GetReportedMessagesAsync();
        }
    }
}