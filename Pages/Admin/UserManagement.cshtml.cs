using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Models;

namespace Snapstagram.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class UserManagementModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<UserWithRoles> Users { get; set; } = new List<UserWithRoles>();
        public string? StatusMessage { get; set; }
        public bool IsSuccess { get; set; } = false;

        public class UserWithRoles
        {
            public string Id { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public List<string> Roles { get; set; } = new List<string>();
        }

        public async Task OnGetAsync()
        {
            await LoadUsersWithRoles();
        }

        public async Task<IActionResult> OnPostMakeAdminAsync(string userId)
        {
            await UpdateUserRole(userId, UserRoles.Administrator, true);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAdminAsync(string userId)
        {
            await UpdateUserRole(userId, UserRoles.Administrator, false);
            return RedirectToPage();
        }

        private async Task UpdateUserRole(string userId, string roleName, bool addRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            if (user == null)
            {
                StatusMessage = "Error: User not found.";
                IsSuccess = false;
                return;
            }

            if (addRole)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    StatusMessage = $"Error: Role {roleName} does not exist.";
                    IsSuccess = false;
                    return;
                }

                if (!await _userManager.IsInRoleAsync(user, roleName))
                {
                    var result = await _userManager.AddToRoleAsync(user, roleName);
                    
                    if (result.Succeeded)
                    {
                        StatusMessage = $"Success: {user.Email} has been assigned the {roleName} role.";
                        IsSuccess = true;
                    }
                    else
                    {
                        StatusMessage = $"Error: Failed to assign {roleName} role. {string.Join(", ", result.Errors.Select(e => e.Description))}";
                        IsSuccess = false;
                    }
                }
                else
                {
                    StatusMessage = $"User already has the {roleName} role.";
                    IsSuccess = true;
                }
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                    
                    if (result.Succeeded)
                    {
                        StatusMessage = $"Success: {roleName} role has been removed from {user.Email}.";
                        IsSuccess = true;
                    }
                    else
                    {
                        StatusMessage = $"Error: Failed to remove {roleName} role. {string.Join(", ", result.Errors.Select(e => e.Description))}";
                        IsSuccess = false;
                    }
                }
                else
                {
                    StatusMessage = $"User does not have the {roleName} role.";
                    IsSuccess = true;
                }
            }

            await LoadUsersWithRoles();
        }

        private async Task LoadUsersWithRoles()
        {
            Users = new List<UserWithRoles>();
            
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var userWithRoles = new UserWithRoles
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                };
                
                Users.Add(userWithRoles);
            }
        }
    }
}
