using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapstagram.Data;
using Snapstagram.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Snapstagram.Controllers
{
    [Route("api/albumphotos")]
    [ApiController]
    [Authorize]
    public class AlbumPhotosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AlbumPhotosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("{photoId}/like")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikePhoto(int photoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || string.IsNullOrEmpty(user.Id)) return Unauthorized();
            var photo = await _context.AlbumPhotos.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == photoId);
            if (photo == null) return NotFound();
            var existingLike = photo.Likes?.FirstOrDefault(l => l.UserId == user.Id);
            bool isLiked;
            if (existingLike == null)
            {
                var like = new AlbumPhotoLike { PhotoId = photoId, UserId = user.Id ?? string.Empty, CreatedAt = System.DateTime.UtcNow };
                _context.AlbumPhotoLikes.Add(like);
                isLiked = true;
            }
            else
            {
                _context.AlbumPhotoLikes.Remove(existingLike);
                isLiked = false;
            }
            await _context.SaveChangesAsync();
            var likeCount = await _context.AlbumPhotoLikes.CountAsync(l => l.PhotoId == photoId);
            return Ok(new { success = true, isLiked, likeCount });
        }

        [HttpPost("{photoId}/comments")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommentPhoto(int photoId, [FromBody] CommentInputModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Content))
                return BadRequest(new { success = false, error = "Content required" });
            var user = await _userManager.GetUserAsync(User);
            var photo = await _context.AlbumPhotos.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == photoId);
            if (photo == null) return NotFound();
            var comment = new AlbumPhotoComment
            {
                PhotoId = photoId,
                UserId = user.Id,
                Content = input.Content,
                CreatedAt = System.DateTime.UtcNow
            };
            _context.AlbumPhotoComments.Add(comment);
            await _context.SaveChangesAsync();
            // Return comment in same shape as JS expects
            var commentResult = new
            {
                id = comment.Id,
                content = comment.Content,
                createdAt = comment.CreatedAt.ToString("MMM dd, yyyy 'at' h:mm tt"),
                user = new
                {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    profilePictureUrl = user.ProfilePictureUrl
                }
            };
            return Ok(new { success = true, comment = commentResult });
        }

        public class CommentInputModel
        {
            public string? Content { get; set; }
        }
    }
}
