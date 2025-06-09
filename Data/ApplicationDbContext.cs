using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Models;

namespace Snapstagram.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<CommentReply> CommentReplies { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Album> Albums { get; set; }
        public DbSet<AlbumPhoto> AlbumPhotos { get; set; }
        public DbSet<AlbumPhotoLike> AlbumPhotoLikes { get; set; }
        public DbSet<AlbumPhotoComment> AlbumPhotoComments { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<ChatGroupMember> ChatGroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<GroupMessageRead> GroupMessageReads { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed roles
            var roles = new[]
            {
                new IdentityRole { Id = "1", Name = "Visitor", NormalizedName = "VISITOR" },
                new IdentityRole { Id = "2", Name = "RegisteredUser", NormalizedName = "REGISTEREDUSER" },
                new IdentityRole { Id = "3", Name = "Administrator", NormalizedName = "ADMINISTRATOR" }
            };

            builder.Entity<IdentityRole>().HasData(roles);

            // Seed default admin user
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = "admin-id",
                UserName = "admin@snapstagram.com",
                NormalizedUserName = "ADMIN@SNAPSTAGRAM.COM",
                Email = "admin@snapstagram.com",
                NormalizedEmail = "ADMIN@SNAPSTAGRAM.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign admin role to admin user
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "3", // Administrator role
                    UserId = "admin-id"
                }
            );

            // Configure Like entity relationships and constraints
            builder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique(); // Ensure one like per user per post

            builder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CommentLike entity relationships and constraints
            builder.Entity<CommentLike>()
                .HasIndex(cl => new { cl.UserId, cl.CommentId })
                .IsUnique(); // Ensure one like per user per comment

            builder.Entity<CommentLike>()
                .HasOne(cl => cl.Comment)
                .WithMany(c => c.CommentLikes)
                .HasForeignKey(cl => cl.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommentLike>()
                .HasOne(cl => cl.User)
                .WithMany()
                .HasForeignKey(cl => cl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CommentReply entity relationships
            builder.Entity<CommentReply>()
                .HasOne(cr => cr.Comment)
                .WithMany(c => c.CommentReplies)
                .HasForeignKey(cr => cr.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommentReply>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Comment entity soft delete (removed global query filter to avoid relationship issues)
            // Note: Soft delete filtering will be handled in application logic

            // Configure CommentReply entity soft delete (removed global query filter to avoid relationship issues)
            // Note: Soft delete filtering will be handled in application logic

            // Configure FriendRequest entity relationships and constraints
            builder.Entity<FriendRequest>()
                .HasIndex(fr => new { fr.SenderId, fr.ReceiverId })
                .IsUnique(); // Ensure one friend request per user pair

            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Album entity relationships
            builder.Entity<Album>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Album>()
                .HasMany(a => a.Photos)
                .WithOne(p => p.Album)
                .HasForeignKey(p => p.AlbumId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlbumPhoto>()
                .HasOne(p => p.DeletedByUser)
                .WithMany()
                .HasForeignKey(p => p.DeletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Album soft delete
            builder.Entity<Album>()
                .HasOne(a => a.DeletedByUser)
                .WithMany()
                .HasForeignKey(a => a.DeletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure AlbumPhotoLike entity relationships
            builder.Entity<AlbumPhotoLike>()
                .HasOne(l => l.Photo)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlbumPhotoLike>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure AlbumPhotoComment entity relationships
            builder.Entity<AlbumPhotoComment>()
                .HasOne(c => c.Photo)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PhotoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AlbumPhotoComment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Message entity relationships
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Recipient)
                .WithMany()
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChatGroup entity relationships
            builder.Entity<ChatGroup>()
                .HasOne(g => g.CreatedByUser)
                .WithMany()
                .HasForeignKey(g => g.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatGroup>()
                .HasOne(g => g.DeletedByUser)
                .WithMany()
                .HasForeignKey(g => g.DeletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ChatGroupMember entity relationships
            builder.Entity<ChatGroupMember>()
                .HasOne(m => m.ChatGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatGroupMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatGroupMember>()
                .HasOne(m => m.AddedByUser)
                .WithMany()
                .HasForeignKey(m => m.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatGroupMember>()
                .HasIndex(m => new { m.ChatGroupId, m.UserId })
                .IsUnique(); // Ensure one membership per user per group

            // Configure GroupMessage entity relationships
            builder.Entity<GroupMessage>()
                .HasOne(m => m.ChatGroup)
                .WithMany(g => g.Messages)
                .HasForeignKey(m => m.ChatGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GroupMessage>()
                .HasOne(m => m.DeletedByUser)
                .WithMany()
                .HasForeignKey(m => m.DeletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GroupMessageRead entity relationships
            builder.Entity<GroupMessageRead>()
                .HasOne(r => r.GroupMessage)
                .WithMany(m => m.ReadByMembers)
                .HasForeignKey(r => r.GroupMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMessageRead>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GroupMessageRead>()
                .HasIndex(r => new { r.GroupMessageId, r.UserId })
                .IsUnique(); // Ensure one read status per user per message
        }
    }
}