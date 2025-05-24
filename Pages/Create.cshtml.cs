using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Services;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Pages
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly IStoryService _storyService;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public CreateModel(IPostService postService, IStoryService storyService, 
            UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _postService = postService;
            _storyService = storyService;
            _userManager = userManager;
            _environment = environment;
        }

        [BindProperty]
        [Required]
        [Display(Name = "Image")]
        public IFormFile ImageFile { get; set; } = null!;

        [BindProperty]
        [Display(Name = "Video")]
        public IFormFile? VideoFile { get; set; }

        [BindProperty]
        [StringLength(2000)]
        public string Caption { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                // Save uploaded files
                var imageUrl = await SaveFileAsync(ImageFile, "images");
                string? videoUrl = null;

                if (VideoFile != null)
                {
                    videoUrl = await SaveFileAsync(VideoFile, "videos");
                }

                // Create post
                await _postService.CreatePostAsync(user.Id, imageUrl, Caption, videoUrl);

                TempData["Success"] = "Post created successfully!";
                return RedirectToPage("/Feed");
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating your post. Please try again.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStoryAsync(IFormFile storyFile, string storyText = "")
        {
            if (storyFile == null)
            {
                TempData["Error"] = "Please select a file for your story.";
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                var isVideo = storyFile.ContentType.StartsWith("video/");
                var folder = isVideo ? "videos" : "images";
                var mediaUrl = await SaveFileAsync(storyFile, folder);
                var mediaType = isVideo ? "video" : "image";

                await _storyService.CreateStoryAsync(user.Id, mediaUrl, mediaType, storyText);

                TempData["Success"] = "Story added successfully!";
                return RedirectToPage("/Feed");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while creating your story. Please try again.";
                return Page();
            }
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{fileName}";
        }
    }
}
