using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Snapstagram.Pages;

[Authorize]
public class GroupsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    public GroupsModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public List<Group> Groups { get; set; } = new();
    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        Groups = await _context.Groups.Where(g => g.Members.Contains(user)).ToListAsync();
    }
}
