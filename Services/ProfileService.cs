using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Services;

public interface IProfileService
{
    Task<User?> GetUserProfileAsync(string userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<Post>> GetUserPostsAsync(string userId, int page = 0, int pageSize = 12);
    Task<int> GetPostsCountAsync(string userId);
    Task<int> GetFollowersCountAsync(string userId);
    Task<int> GetFollowingCountAsync(string userId);
    Task<bool> IsFollowingAsync(string followerId, string followingId);
    Task<bool> ToggleFollowAsync(string followerId, string followingId);
    Task<IEnumerable<User>> GetFollowersAsync(string userId, int page = 0, int pageSize = 20);
    Task<IEnumerable<User>> GetFollowingAsync(string userId, int page = 0, int pageSize = 20);
    Task<bool> UpdateProfileAsync(string userId, string displayName, string bio, bool isPrivate);
    Task<bool> UpdateProfileImageAsync(string userId, string profileImageUrl);
    Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10);
}

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserProfileAsync(string userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<IEnumerable<Post>> GetUserPostsAsync(string userId, int page = 0, int pageSize = 12)
    {
        return await _context.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetPostsCountAsync(string userId)
    {
        return await _context.Posts
            .CountAsync(p => p.UserId == userId);
    }

    public async Task<int> GetFollowersCountAsync(string userId)
    {
        return await _context.Follows
            .CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        return await _context.Follows
            .CountAsync(f => f.FollowerId == userId);
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        return await _context.Follows
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<bool> ToggleFollowAsync(string followerId, string followingId)
    {
        if (followerId == followingId)
            return false;

        var existingFollow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

        if (existingFollow != null)
        {
            _context.Follows.Remove(existingFollow);
        }
        else
        {
            _context.Follows.Add(new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId
            });
        }

        await _context.SaveChangesAsync();
        return existingFollow == null; // Returns true if now following, false if unfollowed
    }

    public async Task<IEnumerable<User>> GetFollowersAsync(string userId, int page = 0, int pageSize = 20)
    {
        return await _context.Follows
            .Where(f => f.FollowingId == userId)
            .Include(f => f.Follower)
            .Select(f => f.Follower)
            .OrderByDescending(u => u.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetFollowingAsync(string userId, int page = 0, int pageSize = 20)
    {
        return await _context.Follows
            .Where(f => f.FollowerId == userId)
            .Include(f => f.Following)
            .Select(f => f.Following)
            .OrderByDescending(u => u.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> UpdateProfileAsync(string userId, string displayName, string bio, bool isPrivate)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.DisplayName = displayName;
        user.Bio = bio;
        user.IsPrivate = isPrivate;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateProfileImageAsync(string userId, string profileImageUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.ProfileImageUrl = profileImageUrl;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, int limit = 10)
    {
        return await _context.Users
            .Where(u => u.UserName!.Contains(searchTerm) || u.DisplayName.Contains(searchTerm))
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }
}
