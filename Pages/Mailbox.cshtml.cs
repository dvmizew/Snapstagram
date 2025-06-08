using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Snapstagram.Pages;

[Authorize]
public class MailboxModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public MailboxModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Conversation> Conversations { get; set; } = new();
    public List<Message> Messages { get; set; } = new();

    public async Task OnGetAsync(int? conversationId)
    {
        var userId = User.Identity.Name;
        Conversations = await _context.Conversations
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .ToListAsync();

        if (conversationId.HasValue)
        {
            Messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId.Value)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }

    public async Task<IActionResult> OnPostAsync(int conversationId, string messageContent)
    {
        var userId = User.Identity.Name;
        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            Content = messageContent,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return RedirectToPage(new { conversationId });
    }
}
