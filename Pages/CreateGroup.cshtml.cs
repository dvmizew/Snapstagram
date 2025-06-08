using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Pages;

[Authorize]
public class CreateGroupModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    public CreateGroupModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    [BindProperty, Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;
    [BindProperty]
    public string? Description { get; set; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var group = new Group { Name = Name, Description = Description };
        group.Members.Add(user);
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();
        return RedirectToPage("/Groups");
    }
}
