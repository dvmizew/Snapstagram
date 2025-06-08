using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using System.Security.Claims;

namespace Snapstagram.Pages
{
    [Authorize]
    public class AlbumsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AlbumsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Album> Albums { get; set; } = [];

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Albums = await _context.Albums
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}
