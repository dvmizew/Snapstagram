using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Services;
using System.ComponentModel.DataAnnotations;

[Authorize]
public class ProfileViewModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IProfileService _profileService;
    private readonly IWebHostEnvironment _environment;

    public ProfileViewModel(UserManager<User> userManager, SignInManager<User> signInManager, IProfileService profileService, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _profileService = profileService;
        _environment = environment;
    }

    public User? ProfileUser { get; set; }
    public User? CurrentUser { get; set; }
    public IEnumerable<Post> Posts { get; set; } = [];
    public bool IsOwnProfile { get; set; }
    public bool IsFollowing { get; set; }
    public bool CanViewPosts { get; set; }
    public int PostsCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public bool HasMorePosts { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    // Settings properties
    [BindProperty]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = "Display Name")]
    [StringLength(50, ErrorMessage = "Display name cannot be longer than 50 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Bio")]
    [StringLength(500, ErrorMessage = "Bio cannot be longer than 500 characters.")]
    public string Bio { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Private Account")]
    public bool IsPrivate { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? username)
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // If no username provided, show current user's profile
        if (string.IsNullOrEmpty(username))
        {
            ProfileUser = CurrentUser;
            IsOwnProfile = true;
        }
        else
        {
            ProfileUser = await _profileService.GetUserByUsernameAsync(username);
            if (ProfileUser == null)
            {
                return NotFound();
            }
            IsOwnProfile = ProfileUser.Id == CurrentUser.Id;
        }

        // Check if current user is following the profile user
        if (!IsOwnProfile)
        {
            IsFollowing = await _profileService.IsFollowingAsync(CurrentUser.Id, ProfileUser.Id);
        }

        // Determine if current user can view posts
        CanViewPosts = IsOwnProfile || !ProfileUser.IsPrivate || IsFollowing;

        // Load profile statistics
        PostsCount = await _profileService.GetPostsCountAsync(ProfileUser.Id);
        FollowersCount = await _profileService.GetFollowersCountAsync(ProfileUser.Id);
        FollowingCount = await _profileService.GetFollowingCountAsync(ProfileUser.Id);

        // Load posts if user can view them
        if (CanViewPosts)
        {
            var posts = await _profileService.GetUserPostsAsync(ProfileUser.Id, 0, 12);
            Posts = posts;
            HasMorePosts = posts.Count() == 12;
        }

        // Populate settings data for own profile
        if (IsOwnProfile)
        {
            Username = CurrentUser.UserName ?? "";
            Email = CurrentUser.Email ?? "";
            DisplayName = CurrentUser.DisplayName;
            Bio = CurrentUser.Bio;
            IsPrivate = CurrentUser.IsPrivate;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostToggleFollowAsync(string targetUserId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var isNowFollowing = await _profileService.ToggleFollowAsync(currentUser.Id, targetUserId);
        
        // Update the follow status for the page
        IsFollowing = isNowFollowing;
        
        // Redirect back to the profile
        var targetUser = await _profileService.GetUserProfileAsync(targetUserId);
        return RedirectToPage("/ProfileView", new { username = targetUser?.UserName });
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Preserve username and email for display
        Username = CurrentUser.UserName ?? "";
        Email = CurrentUser.Email ?? "";

        // Validate only the editable fields
        ModelState.Remove("CurrentPassword");
        ModelState.Remove("NewPassword");
        ModelState.Remove("ConfirmNewPassword");

        if (!ModelState.IsValid)
        {
            return await OnGetAsync(null);
        }

        var success = await _profileService.UpdateProfileAsync(CurrentUser.Id, DisplayName, Bio, IsPrivate);
        
        if (success)
        {
            // Update the current user object for display
            CurrentUser.DisplayName = DisplayName;
            CurrentUser.Bio = Bio;
            CurrentUser.IsPrivate = IsPrivate;
            
            Message = "Profile updated successfully!";
            IsSuccess = true;
        }
        else
        {
            Message = "Failed to update profile.";
            IsSuccess = false;
        }

        return await OnGetAsync(null);
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Preserve profile data for display
        Username = CurrentUser.UserName ?? "";
        Email = CurrentUser.Email ?? "";
        DisplayName = CurrentUser.DisplayName;
        Bio = CurrentUser.Bio;
        IsPrivate = CurrentUser.IsPrivate;

        // Validate only password fields
        ModelState.Remove("DisplayName");
        ModelState.Remove("Bio");

        if (!ModelState.IsValid)
        {
            return await OnGetAsync(null);
        }

        var result = await _userManager.ChangePasswordAsync(CurrentUser, CurrentPassword, NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(CurrentUser);
            Message = "Password changed successfully!";
            IsSuccess = true;
            
            // Clear password fields
            CurrentPassword = "";
            NewPassword = "";
            ConfirmNewPassword = "";
        }
        else
        {
            Message = "Failed to change password: " + string.Join(", ", result.Errors.Select(e => e.Description));
            IsSuccess = false;
        }

        return await OnGetAsync(null);
    }

    public async Task<IActionResult> OnPostUploadProfileImageAsync(IFormFile profileImage)
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Preserve profile data for display
        Username = CurrentUser.UserName ?? "";
        Email = CurrentUser.Email ?? "";
        DisplayName = CurrentUser.DisplayName;
        Bio = CurrentUser.Bio;
        IsPrivate = CurrentUser.IsPrivate;

        if (profileImage == null || profileImage.Length == 0)
        {
            Message = "Please select an image file.";
            IsSuccess = false;
            return await OnGetAsync(null);
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        if (!allowedTypes.Contains(profileImage.ContentType.ToLower()))
        {
            Message = "Please upload a valid image file (JPEG, PNG, or GIF).";
            IsSuccess = false;
            return await OnGetAsync(null);
        }

        // Validate file size (5MB max)
        if (profileImage.Length > 5 * 1024 * 1024)
        {
            Message = "Image file must be smaller than 5MB.";
            IsSuccess = false;
            return await OnGetAsync(null);
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsPath);

            // Delete old profile image if it exists and is not the default
            if (!string.IsNullOrEmpty(CurrentUser.ProfileImageUrl) && 
                !CurrentUser.ProfileImageUrl.Contains("default-avatar"))
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, CurrentUser.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Generate unique filename
            var fileExtension = Path.GetExtension(profileImage.FileName);
            var fileName = $"{CurrentUser.Id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
            }

            // Update user's profile image URL
            var imageUrl = $"/uploads/profiles/{fileName}";
            var success = await _profileService.UpdateProfileImageAsync(CurrentUser.Id, imageUrl);

            if (success)
            {
                CurrentUser.ProfileImageUrl = imageUrl;
                Message = "Profile image updated successfully!";
                IsSuccess = true;
            }
            else
            {
                Message = "Failed to update profile image.";
                IsSuccess = false;
            }
        }
        catch (Exception)
        {
            Message = "An error occurred while uploading the image.";
            IsSuccess = false;
        }

        return await OnGetAsync(null);
    }

    public async Task<IActionResult> OnPostDownloadDataAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        try
        {
            // Gather user data
            var userData = new
            {
                Profile = new
                {
                    Username = CurrentUser.UserName,
                    Email = CurrentUser.Email,
                    DisplayName = CurrentUser.DisplayName,
                    Bio = CurrentUser.Bio,
                    IsPrivate = CurrentUser.IsPrivate,
                    CreatedAt = CurrentUser.CreatedAt,
                    ProfileImageUrl = CurrentUser.ProfileImageUrl
                },
                Posts = await _profileService.GetUserPostsAsync(CurrentUser.Id, 0, 1000),
                Followers = await _profileService.GetFollowersAsync(CurrentUser.Id, 0, 1000),
                Following = await _profileService.GetFollowingAsync(CurrentUser.Id, 0, 1000),
                Statistics = new
                {
                    PostsCount = await _profileService.GetPostsCountAsync(CurrentUser.Id),
                    FollowersCount = await _profileService.GetFollowersCountAsync(CurrentUser.Id),
                    FollowingCount = await _profileService.GetFollowingCountAsync(CurrentUser.Id)
                },
                ExportDate = DateTime.UtcNow
            };

            // Convert to JSON
            var json = System.Text.Json.JsonSerializer.Serialize(userData, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            // Return as downloadable file
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var fileName = $"snapstagram-data-{CurrentUser.UserName}-{DateTime.UtcNow:yyyyMMdd}.json";
            
            return File(bytes, "application/json", fileName);
        }
        catch (Exception)
        {
            Message = "Failed to generate data export. Please try again.";
            IsSuccess = false;
            return await OnGetAsync(null);
        }
    }

    public async Task<IActionResult> OnPostDeleteAccountAsync(string confirmPassword)
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Verify password
        var passwordCheck = await _userManager.CheckPasswordAsync(CurrentUser, confirmPassword);
        if (!passwordCheck)
        {
            Message = "Incorrect password. Account deletion cancelled.";
            IsSuccess = false;
            return await OnGetAsync(null);
        }

        try
        {
            // Delete user's uploaded files
            var userUploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (Directory.Exists(userUploadsPath))
            {
                var userFiles = Directory.GetFiles(userUploadsPath, $"{CurrentUser.Id}_*", SearchOption.AllDirectories);
                foreach (var file in userFiles)
                {
                    System.IO.File.Delete(file);
                }
            }

            // Delete the user account
            var result = await _userManager.DeleteAsync(CurrentUser);
            if (result.Succeeded)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage("/Index", new { message = "Account deleted successfully." });
            }
            else
            {
                Message = "Failed to delete account: " + string.Join(", ", result.Errors.Select(e => e.Description));
                IsSuccess = false;
            }
        }
        catch (Exception)
        {
            Message = "An error occurred while deleting the account.";
            IsSuccess = false;
        }

        return await OnGetAsync(null);
    }

}
