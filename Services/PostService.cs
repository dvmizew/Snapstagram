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
}
