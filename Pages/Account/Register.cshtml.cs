using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using System.ComponentModel.DataAnnotations;

public class RegisterModel : PageModel
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public RegisterModel(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    [Required]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        // Check if username already exists
        var existingUser = await _userManager.FindByNameAsync(Username);
        if (existingUser != null)
        {
            ErrorMessage = "Username already exists.";
            return Page();
        }

        // Check if email already exists
        var existingUserByEmail = await _userManager.FindByEmailAsync(Email);
        if (existingUserByEmail != null)
        {
            ErrorMessage = "An account with this email already exists.";
            return Page();
        }

        var user = new User
        {
            UserName = Username,
            Email = Email,
            DisplayName = FullName,
            Bio = "",
            ProfileImageUrl = "/images/default-avatar.png",
            IsPrivate = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl ?? "/");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}
