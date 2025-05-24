using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Services;

public interface IStoryService
{
    Task<IEnumerable<Story>> GetActiveStoriesAsync(string userId);
    Task<Story> CreateStoryAsync(string userId, string mediaUrl, string mediaType, string text = "");
    Task<bool> ViewStoryAsync(int storyId, string userId);
    Task<IEnumerable<Story>> GetUserStoriesAsync(string userId);
}

public class StoryService(ApplicationDbContext context) : IStoryService
{
    private static DateTime CutoffTime => DateTime.UtcNow;

    public async Task<IEnumerable<Story>> GetActiveStoriesAsync(string userId)
    {
        var followingIds = await context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(userId);

        return await context.Stories
            .Where(s => followingIds.Contains(s.UserId) && s.ExpiresAt > CutoffTime)
            .Include(s => s.User)
            .Include(s => s.Views)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Story> CreateStoryAsync(string userId, string mediaUrl, string mediaType, string text = "")
    {
        var story = new Story
        {
            UserId = userId,
            MediaUrl = mediaUrl,
            MediaType = mediaType,
            Text = text
        };

        context.Stories.Add(story);
        await context.SaveChangesAsync();
        return story;
    }

    public async Task<bool> ViewStoryAsync(int storyId, string userId)
    {
        var existingView = await context.StoryViews
            .FirstOrDefaultAsync(sv => sv.StoryId == storyId && sv.UserId == userId);

        if (existingView != null) return false;

        context.StoryViews.Add(new StoryView { StoryId = storyId, UserId = userId });

        if (await context.Stories.FindAsync(storyId) is { } story)
            story.ViewsCount++;

        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Story>> GetUserStoriesAsync(string userId) =>
        await context.Stories
            .Where(s => s.UserId == userId && s.ExpiresAt > CutoffTime)
            .Include(s => s.Views).ThenInclude(v => v.User)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
}
