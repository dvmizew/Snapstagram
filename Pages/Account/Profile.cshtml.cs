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

            [Display(Name = "Profile is Public")]
            public bool IsProfilePublic { get; set; }
        }

        public class PostInputModel
        {
            [Display(Name = "What's on your mind?")]
            [StringLength(2000)]
            public string Content { get; set; } = string.Empty;

            [Display(Name = "Photo")]
            public IFormFile? Photo { get; set; }
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
                    Email = CurrentUser.Email,
                    PhoneNumber = CurrentUser.PhoneNumber,
                    DateOfBirth = CurrentUser.DateOfBirth,
                    Location = CurrentUser.Location,
                    Website = CurrentUser.Website,
                    Occupation = CurrentUser.Occupation,
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

            // Validate that either content or photo is provided
            if (string.IsNullOrWhiteSpace(PostInput.Content) && PostInput.Photo == null)
            {
                ModelState.AddModelError("PostInput.Content", "Please enter some text or upload a photo.");
            }

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
