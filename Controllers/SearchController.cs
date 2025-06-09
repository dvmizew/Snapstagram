using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SearchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return Ok(new List<object>());
            }

            query = query.ToLower().Trim();
            bool isAuthenticated = User.Identity?.IsAuthenticated == true;

            // Build the base query
            var baseQuery = _context.Users
                .Where(u => u.IsActive && 
                           ((u.FirstName != null && u.FirstName.ToLower().Contains(query)) ||
                            (u.LastName != null && u.LastName.ToLower().Contains(query)) ||
                            (u.Email != null && u.Email.ToLower().Contains(query)) ||
                            (u.FirstName != null && u.LastName != null && 
                             (u.FirstName + " " + u.LastName).ToLower().Contains(query))));

            // If user is not authenticated, only show public profiles
            if (!isAuthenticated)
            {
                baseQuery = baseQuery.Where(u => u.IsProfilePublic);
            }

            var users = await baseQuery
                .Select(u => new
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = isAuthenticated ? u.Email : null, // Hide email for non-authenticated users
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    IsProfilePublic = u.IsProfilePublic
                })
                .Take(10)
                .ToListAsync();

            return Ok(users);
        }
    }
}
