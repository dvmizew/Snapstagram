using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Pages
{
    [Authorize]
    public class FeedModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly IStoryService _storyService;
        private readonly UserManager<User> _userManager;

        public FeedModel(IPostService postService, IStoryService storyService, UserManager<User> userManager)
        {
            _postService = postService;
            _storyService = storyService;
            _userManager = userManager;
        }

        public IEnumerable<Post> Posts { get; set; } = new List<Post>();
        public IEnumerable<Story> Stories { get; set; } = new List<Story>();
        public HashSet<int> UserLikes { get; set; } = new HashSet<int>();
        public HashSet<int> UserBookmarks { get; set; } = new HashSet<int>();
        public string CurrentUserAvatar { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            Posts = await _postService.GetFeedPostsAsync(user.Id);
            Stories = await _storyService.GetActiveStoriesAsync(user.Id);

            // Get user's likes for the posts
            UserLikes = Posts
                .SelectMany(p => p.Likes)
                .Where(l => l.UserId == user.Id)
                .Select(l => l.PostId)
                .ToHashSet();

            // Get user's bookmarks for the posts
            UserBookmarks = new HashSet<int>();
            foreach (var post in Posts)
            {
                if (await _postService.HasUserBookmarkedPostAsync(post.Id, user.Id))
                {
                    UserBookmarks.Add(post.Id);
                }
            }

            CurrentUserAvatar = user.ProfileImageUrl ?? GetDefaultAvatar(user.DisplayName ?? user.UserName ?? "U");

            return Page();
        }

        // Helper methods for the view
        public static string GetDefaultAvatar(string name)
        {
            var initial = !string.IsNullOrEmpty(name) ? name[0].ToString().ToUpper() : "U";
            return $"https://via.placeholder.com/40x40/6c757d/ffffff?text={initial}";
        }

        public static string GetRelativeTime(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes}m";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays}d";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)}w";
            
            return dateTime.ToString("MMM dd");
        }
    }
}
