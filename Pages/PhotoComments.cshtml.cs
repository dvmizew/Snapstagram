using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snapstagram.Models;
using Snapstagram.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Pages;

[Authorize]
public class PhotoCommentsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    public PhotoCommentsModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public int PhotoId { get; set; }
    [BindProperty]
    [Required, StringLength(500)]
    public string CommentText { get; set; } = string.Empty;
    public Photo? Photo { get; set; }
    public List<PhotoComment> Comments { get; set; } = new();
    public bool IsOwner { get; set; }

    public async Task OnGetAsync(int photoId)
    {
        PhotoId = photoId;
        Photo = await _context.Photos.Include(p => p.Album).FirstOrDefaultAsync(p => p.Id == photoId);
        if (Photo != null)
        {
            Comments = await _context.PhotoComments.Include(c => c.User).Where(c => c.PhotoId == photoId).ToListAsync();
            var user = await _userManager.GetUserAsync(User);
            IsOwner = user != null && Photo.Album?.UserId == user.Id;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || string.IsNullOrWhiteSpace(CommentText)) return Page();
        var comment = new PhotoComment { PhotoId = PhotoId, UserId = user.Id, Text = CommentText, IsAccepted = false };
        _context.PhotoComments.Add(comment);
        await _context.SaveChangesAsync();
        return RedirectToPage(new { photoId = PhotoId });
    }

    public async Task<IActionResult> OnPostAcceptAsync(int commentId)
    {
        var comment = await _context.PhotoComments.FindAsync(commentId);
        if (comment != null)
        {
            comment.IsAccepted = true;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { photoId = comment?.PhotoId });
    }
}
