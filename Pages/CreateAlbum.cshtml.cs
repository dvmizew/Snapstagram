using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Data;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Pages;

[Authorize]
public class CreateAlbumModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    public CreateAlbumModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty, Required, StringLength(50)]
    public string Title { get; set; } = string.Empty;
    [BindProperty]
    public string? Description { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var album = new Album { UserId = user.Id, Title = Title, Description = Description };
        _context.Albums.Add(album);
        await _context.SaveChangesAsync();
        return RedirectToPage("/Albums");
    }
}
