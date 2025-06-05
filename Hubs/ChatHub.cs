using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly NotificationService _notificationService;

    public ChatHub(ApplicationDbContext context, UserManager<User> userManager, NotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    public async Task SendMessage(string receiverId, string messageContent, string? mediaUrl = null)
    {
        var senderId = Context.UserIdentifier;
        if (senderId == null) return;

        var sender = await _userManager.FindByIdAsync(senderId);
        if (sender == null) return;

        var receiver = await _userManager.FindByIdAsync(receiverId);
        if (receiver == null) return;

        // Create message in database
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = messageContent,
            MediaUrl = mediaUrl
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Create notification
        await _notificationService.CreateMessageNotificationAsync(receiverId, senderId, message.Id);

        var messageData = new
        {
            id = message.Id,
            content = message.Content,
            mediaUrl = message.MediaUrl,
            sentAt = message.SentAt,
            sender = new
            {
                id = sender.Id,
                displayName = sender.DisplayName,
                profileImageUrl = sender.ProfileImageUrl
            }
        };

        // Send to specific user
        await Clients.User(receiverId).SendAsync("ReceiveMessage", messageData);
        
        // Also send back to sender for confirmation
        await Clients.Caller.SendAsync("MessageSent", messageData);
    }

    public async Task MarkMessageAsRead(int messageId)
    {
        var userId = Context.UserIdentifier;
        if (userId == null) return;

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == userId);

        if (message != null && !message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Notify sender that message was read
            await Clients.User(message.SenderId).SendAsync("MessageRead", messageId, message.ReadAt);
        }
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            // Join user to their personal room for notifications
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }
}
