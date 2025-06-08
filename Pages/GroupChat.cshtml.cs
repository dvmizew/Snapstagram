using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Pages;

[Authorize]
public class GroupChatModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    public GroupChatModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    [BindProperty]
    public int GroupId { get; set; }
    [BindProperty, StringLength(500)]
    public string? MessageText { get; set; }
    public void OnGet(int groupId) { GroupId = groupId; }
    public async Task<IActionResult> OnPostAsync()
    {
        // TODO: Save group message to DB
        return RedirectToPage(new { groupId = GroupId });
    }
}
