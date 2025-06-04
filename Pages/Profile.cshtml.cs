using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using System.ComponentModel.DataAnnotations;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public ProfileModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Bio")]
    public string Bio { get; set; } = string.Empty;

    [BindProperty]
    [Display(Name = "Private Account")]
    public bool IsPrivate { get; set; }

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        FullName = user.DisplayName;
        Bio = user.Bio ?? "";
        IsPrivate = user.IsPrivate;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        user.DisplayName = FullName;
        user.Bio = Bio;
        user.IsPrivate = IsPrivate;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            IsSuccess = true;
            Message = "Profile updated successfully!";
        }
        else
        {
            IsSuccess = false;
            Message = "Failed to update profile.";
        }

        return Page();
    }
}
