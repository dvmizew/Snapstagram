using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public ApplicationUser CurrentUser { get; set; } = default!;
        public List<Post> Posts { get; set; } = new List<Post>();
        public bool IsOwnProfile { get; set; }
        public bool CanViewProfile { get; set; }
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        [BindProperty]
        public PostInputModel PostInput { get; set; } = default!;

        public class InputModel
        {
            [Display(Name = "First Name")]
            [StringLength(50)]
            public string? FirstName { get; set; }

            [Display(Name = "Last Name")]
            [StringLength(50)]
            public string? LastName { get; set; }

            [Display(Name = "Bio")]
            [StringLength(500)]
            public string? Bio { get; set; }

            [Display(Name = "Profile is Public")]
            public bool IsProfilePublic { get; set; }
        }

        public class PostInputModel
        {
            [Required]
            [Display(Name = "What's on your mind?")]
            [StringLength(2000)]
            public string Content { get; set; } = string.Empty;
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

            // Check if user can view this profile
            CanViewProfile = IsOwnProfile || targetUser.IsProfilePublic;

            if (CanViewProfile)
            {
                // Load user's posts
                Posts = await _context.Posts
                    .Where(p => p.UserId == targetUser.Id && p.IsActive && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedAt)
                    .Include(p => p.Comments.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.User)
                    .ToListAsync();
            }

            // Initialize input model for editing (only if own profile)
            if (IsOwnProfile)
            {
                Input = new InputModel
                {
                    FirstName = CurrentUser.FirstName,
                    LastName = CurrentUser.LastName,
                    Bio = CurrentUser.Bio,
                    IsProfilePublic = CurrentUser.IsProfilePublic
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                CurrentUser = user;
                IsOwnProfile = true;
                CanViewProfile = true;
                await LoadPostsAsync(user.Id);
                return Page();
            }

            // Update user properties
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.Bio = Input.Bio;
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

            if (!ModelState.IsValid)
            {
                CurrentUser = user;
                IsOwnProfile = true;
                CanViewProfile = true;
                await LoadPostsAsync(user.Id);
                return Page();
            }

            var post = new Post
            {
                UserId = user.Id,
                Caption = PostInput.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            StatusMessage = "Your post has been created successfully.";
            return RedirectToPage();
        }

        private async Task LoadPostsAsync(string userId)
        {
            Posts = await _context.Posts
                .Where(p => p.UserId == userId && p.IsActive && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Include(p => p.Comments.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.User)
                .ToListAsync();
        }
    }
}
