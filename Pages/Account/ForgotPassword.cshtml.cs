using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using System.ComponentModel.DataAnnotations;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<User> _userManager;

    public ForgotPasswordModel(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            IsSuccess = true;
            Message = "If an account with that email exists, we've sent you a password reset link.";
            return Page();
        }

        // In a real application, you would send an email here
        // For now, we'll just show a success message
        IsSuccess = true;
        Message = "If an account with that email exists, we've sent you a password reset link.";
        
        return Page();
    }
}
