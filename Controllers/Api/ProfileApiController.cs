using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Snapstagram.Controllers.Api
{
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileApiController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileApiController(UserManager<IdentityUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            try
            {
                if (profileImage == null || profileImage.Length == 0)
                {
                    return BadRequest(new { message = "No file selected" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(profileImage.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "Invalid file type. Please select a JPEG, PNG, or GIF file." });
                }

                // Validate file size (5MB)
                if (profileImage.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "File size must be less than 5MB" });
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profile-images");
                Directory.CreateDirectory(uploadsPath);

                // Generate unique filename
                var fileExtension = Path.GetExtension(profileImage.FileName);
                var fileName = $"{user.Id}_{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Generate URL for the uploaded image
                var imageUrl = $"/uploads/profile-images/{fileName}";

                // For now, we'll just return success - in a real app you'd save this to a user profile table
                return Ok(new { 
                    imageUrl = imageUrl,
                    message = "Profile image updated successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while uploading the image" });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                // For now, just return success - in a real app you'd update the user profile
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while updating the profile" });
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Password changed successfully" });
                }
                else
                {
                    return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while changing the password" });
            }
        }
    }

    public class UpdateProfileRequest
    {
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
}