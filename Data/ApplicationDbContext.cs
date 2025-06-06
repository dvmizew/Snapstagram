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
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed roles
            var roles = new[]
            {
                new IdentityRole { Id = "1", Name = "Visitor", NormalizedName = "VISITOR" },
                new IdentityRole { Id = "2", Name = "RegisteredUser", NormalizedName = "REGISTEREDUSER" },
                new IdentityRole { Id = "3", Name = "Moderator", NormalizedName = "MODERATOR" },
                new IdentityRole { Id = "4", Name = "Administrator", NormalizedName = "ADMINISTRATOR" }
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
                    RoleId = "4", // Administrator role
                    UserId = "admin-id"
                }
            );
        }
    }
}