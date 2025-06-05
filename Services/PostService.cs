using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;

namespace Snapstagram.Services;

public interface IPostService
{
    Task<IEnumerable<Post>> GetFeedPostsAsync(string userId, int page = 0, int pageSize = 10);
    Task<Post?> GetPostByIdAsync(int postId);
    Task<Post> CreatePostAsync(string userId, string imageUrl, string caption, string? videoUrl = null);
    Task<bool> ToggleLikeAsync(int postId, string userId);
    Task<Comment> AddCommentAsync(int postId, string userId, string text);
    Task<bool> HasUserLikedPostAsync(int postId, string userId);
    Task<bool> ToggleBookmarkAsync(int postId, string userId);
    Task<bool> HasUserBookmarkedPostAsync(int postId, string userId);
    Task<IEnumerable<Post>> GetBookmarkedPostsAsync(string userId, int page = 0, int pageSize = 10);
}

public class PostService(ApplicationDbContext context) : IPostService
{
    public async Task<IEnumerable<Post>> GetFeedPostsAsync(string userId, int page = 0, int pageSize = 10)
    {
        var followingIds = await context.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();

        followingIds.Add(userId);

        return await context.Posts
            .Where(p => followingIds.Contains(p.UserId))
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments.OrderBy(c => c.CreatedAt).Take(3))
                .ThenInclude(c => c.User)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(int postId) =>
        await context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes).ThenInclude(l => l.User)
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == postId);

    public async Task<Post> CreatePostAsync(string userId, string imageUrl, string caption, string? videoUrl = null)
    {
        var post = new Post
        {
            UserId = userId,
            ImageUrl = imageUrl,
            Caption = caption,
            VideoUrl = videoUrl
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post;
    }

    public async Task<bool> ToggleLikeAsync(int postId, string userId)
    {
        var existingLike = await context.Likes
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

        var post = await context.Posts.FindAsync(postId);
        if (post == null) return false;

        if (existingLike != null)
        {
            context.Likes.Remove(existingLike);
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
        }
        else
        {
            context.Likes.Add(new Like { PostId = postId, UserId = userId });
            post.LikesCount++;
        }

        await context.SaveChangesAsync();
        return existingLike == null;
    }

    public async Task<Comment> AddCommentAsync(int postId, string userId, string text)
    {
        var comment = new Comment { PostId = postId, UserId = userId, Text = text };
        context.Comments.Add(comment);

        if (await context.Posts.FindAsync(postId) is { } post)
            post.CommentsCount++;

        await context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> HasUserLikedPostAsync(int postId, string userId)
    {
        return await context.Likes
            .AnyAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task<bool> ToggleBookmarkAsync(int postId, string userId)
    {
        var existingBookmark = await context.Bookmarks
            .FirstOrDefaultAsync(b => b.PostId == postId && b.UserId == userId);

        if (existingBookmark != null)
        {
            context.Bookmarks.Remove(existingBookmark);
        }
        else
        {
            context.Bookmarks.Add(new Bookmark { PostId = postId, UserId = userId });
        }

        await context.SaveChangesAsync();
        return existingBookmark == null; // Return true if bookmarked, false if unbookmarked
    }

    public async Task<bool> HasUserBookmarkedPostAsync(int postId, string userId)
    {
        return await context.Bookmarks
            .AnyAsync(b => b.PostId == postId && b.UserId == userId);
    }

    public async Task<IEnumerable<Post>> GetBookmarkedPostsAsync(string userId, int page = 0, int pageSize = 10)
    {
        return await context.Bookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.Post)
                .ThenInclude(p => p.User)
            .Include(b => b.Post)
                .ThenInclude(p => p.Likes)
            .Include(b => b.Post)
                .ThenInclude(p => p.Comments.OrderBy(c => c.CreatedAt).Take(3))
                .ThenInclude(c => c.User)
            .OrderByDescending(b => b.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Select(b => b.Post)
            .AsNoTracking()
            .ToListAsync();
    }
}
