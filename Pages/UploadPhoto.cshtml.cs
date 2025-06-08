using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Snapstagram.Models;
using Snapstagram.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Snapstagram.Pages;

[Authorize]
public class UploadPhotoModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _env;
    public UploadPhotoModel(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    [BindProperty, Required]
    public int AlbumId { get; set; }
    [BindProperty, Required]
    public IFormFile? PhotoFile { get; set; }
    [BindProperty]
    public string? Caption { get; set; }
    public List<Album> UserAlbums { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        UserAlbums = _context.Albums.Where(a => a.UserId == user.Id).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || PhotoFile == null || AlbumId == 0) return Page();
        var album = await _context.Albums.FindAsync(AlbumId);
        if (album == null || album.UserId != user.Id) return Page();
        var uploads = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid()}_{PhotoFile.FileName}";
        var filePath = Path.Combine(uploads, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await PhotoFile.CopyToAsync(stream);
        }
        var photo = new Photo { AlbumId = AlbumId, Url = "/uploads/" + fileName, Caption = Caption };
        _context.Photos.Add(photo);
        await _context.SaveChangesAsync();
        return RedirectToPage("/Albums");
    }
}
