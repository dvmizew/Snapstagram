using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Services;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Pages;

[Authorize]
public class SettingsModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IProfileService _profileService;
    private readonly IWebHostEnvironment _environment;

    public SettingsModel(UserManager<User> userManager, SignInManager<User> signInManager, 
        IProfileService profileService, IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _profileService = profileService;
        _environment = environment;
    }

    public User? CurrentUser { get; set; }
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public string? ActiveTab { get; set; } = "general";
    
    // Statistics properties
    public int PostsCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }

    // General profile settings
    [BindProperty]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Email")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
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

    // Password change properties
    [BindProperty]
    [Required(ErrorMessage = "Current password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;

    // Delete account property
    [BindProperty]
    [Required(ErrorMessage = "Password is required to delete your account.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    public string DeleteConfirmPassword { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? tab = "general")
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Set active tab
        ActiveTab = tab;

        // Load current user data
        Username = CurrentUser.UserName ?? string.Empty;
        Email = CurrentUser.Email ?? string.Empty;
        DisplayName = CurrentUser.DisplayName;
        Bio = CurrentUser.Bio;
        IsPrivate = CurrentUser.IsPrivate;
        
        // Load statistics
        PostsCount = await _profileService.GetPostsCountAsync(CurrentUser.Id);
        FollowersCount = await _profileService.GetFollowersCountAsync(CurrentUser.Id);
        FollowingCount = await _profileService.GetFollowingCountAsync(CurrentUser.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Validate only the profile fields
        ModelState.Remove("CurrentPassword");
        ModelState.Remove("NewPassword");
        ModelState.Remove("ConfirmNewPassword");
        ModelState.Remove("DeleteConfirmPassword");

        if (!ModelState.IsValid)
        {
            ActiveTab = "general";
            return Page();
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

        ActiveTab = "general";
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateEmailAsync(string newEmail)
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Load the user data for form fields
        Username = CurrentUser.UserName ?? string.Empty;
        DisplayName = CurrentUser.DisplayName;
        Bio = CurrentUser.Bio;
        IsPrivate = CurrentUser.IsPrivate;
        ActiveTab = "general";

        if (string.IsNullOrWhiteSpace(newEmail))
        {
            Message = "Email cannot be empty.";
            IsSuccess = false;
            return Page();
        }

        var user = await _userManager.FindByIdAsync(CurrentUser.Id);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // In a production app, you'd typically:
        // 1. Send a confirmation email with a token
        // 2. Have the user click that token to verify their email
        // For this demo, we'll just update it directly
        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

        if (result.Succeeded)
        {
            // Also update the username if it's an email
            if (user.UserName?.Contains('@') == true)
            {
                await _userManager.SetUserNameAsync(user, newEmail);
                await _signInManager.RefreshSignInAsync(user);
            }
            Email = newEmail;
            Message = "Email updated successfully!";
            IsSuccess = true;
        }
        else
        {
            Message = "Failed to update email: " + string.Join(", ", result.Errors.Select(e => e.Description));
            IsSuccess = false;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Load the user data for form fields
        Username = CurrentUser.UserName ?? string.Empty;
        Email = CurrentUser.Email ?? string.Empty;
        DisplayName = CurrentUser.DisplayName;
        Bio = CurrentUser.Bio;
        IsPrivate = CurrentUser.IsPrivate;
        ActiveTab = "security";

        // Validate only password fields
        ModelState.Remove("DisplayName");
        ModelState.Remove("Bio");
        ModelState.Remove("DeleteConfirmPassword");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _userManager.ChangePasswordAsync(CurrentUser, CurrentPassword, NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(CurrentUser);
            Message = "Password changed successfully!";
            IsSuccess = true;
            
            // Clear password fields
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }
        else
        {
            Message = "Failed to change password: " + string.Join(", ", result.Errors.Select(e => e.Description));
            IsSuccess = false;
        }

        return Page();
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
            // Gather user data - same as in ProfileViewModel
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
        catch (Exception ex)
        {
            // Load the user data for form fields
            Username = CurrentUser.UserName ?? string.Empty;
            Email = CurrentUser.Email ?? string.Empty;
            DisplayName = CurrentUser.DisplayName;
            Bio = CurrentUser.Bio;
            IsPrivate = CurrentUser.IsPrivate;
            ActiveTab = "data";
            
            Message = $"Failed to generate data export: {ex.Message}";
            IsSuccess = false;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAccountAsync()
    {
        CurrentUser = await _userManager.GetUserAsync(User);
        if (CurrentUser == null)
        {
            return RedirectToPage("/Account/Login");
        }

        // Load the user data for form fields
        Username = CurrentUser.UserName ?? string.Empty;
        Email = CurrentUser.Email ?? string.Empty;
        DisplayName = CurrentUser.DisplayName;
        Bio = CurrentUser.Bio;
        IsPrivate = CurrentUser.IsPrivate;
        ActiveTab = "delete";

        // Verify password
        var passwordCheck = await _userManager.CheckPasswordAsync(CurrentUser, DeleteConfirmPassword);
        if (!passwordCheck)
        {
            Message = "Incorrect password. Account deletion cancelled.";
            IsSuccess = false;
            return Page();
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
        catch (Exception ex)
        {
            Message = $"An error occurred while deleting the account: {ex.Message}";
            IsSuccess = false;
        }

        return Page();
    }
}
