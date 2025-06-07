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
        }
    }
}