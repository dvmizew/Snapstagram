using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Snapstagram.Pages;

[Authorize]
public class GroupMessagesModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public GroupMessagesModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Group> Groups { get; set; } = new();
    public List<Message> Messages { get; set; } = new();

    public async Task OnGetAsync(int? groupId)
    {
        var userId = User.Identity.Name;
        Groups = await _context.Groups
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .ToListAsync();

        if (groupId.HasValue)
        {
            Messages = await _context.Messages
                .Where(m => m.GroupId == groupId.Value)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }

    public async Task<IActionResult> OnPostAsync(int groupId, string messageContent)
    {
        var userId = User.Identity.Name;
        var message = new Message
        {
            GroupId = groupId,
            SenderId = userId,
            Content = messageContent,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { groupId });
    }
}
