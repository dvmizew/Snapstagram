using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using Snapstagram.Services;
using Snapstagram.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddDefaultIdentity<User>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = options.Password.RequireNonAlphanumeric = 
        options.Password.RequireUppercase = options.Password.RequireLowercase = false;
}).AddEntityFrameworkStores<ApplicationDbContext>();

// Services
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment()) 
    app.UseExceptionHandler("/Error").UseHsts();

app.UseHttpsRedirection()
   .UseStaticFiles()
   .UseRouting()
   .UseAuthentication()
   .UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

// Initialize database
using var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreatedAsync();

app.Run();
