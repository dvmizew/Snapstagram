using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Models;
using Snapstagram.Services;

namespace Snapstagram.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly NotificationService _notificationService;

    public MessagesController(ApplicationDbContext context, UserManager<User> userManager, NotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Message content is required");

        var receiver = await _userManager.FindByIdAsync(request.ReceiverId);
        if (receiver == null) return NotFound("Receiver not found");

        var message = new Message
        {
            SenderId = currentUser.Id,
            ReceiverId = request.ReceiverId,
            Content = request.Content,
            MediaUrl = request.MediaUrl
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Create notification for the receiver
        await _notificationService.CreateMessageNotificationAsync(request.ReceiverId, currentUser.Id, message.Id);

        return Ok(new
        {
            id = message.Id,
            content = message.Content,
            mediaUrl = message.MediaUrl,
            sentAt = message.SentAt,
            senderId = message.SenderId,
            receiverId = message.ReceiverId
        });
    }

    [HttpGet("conversation/{userId}")]
    public async Task<IActionResult> GetConversation(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var messages = await _context.Messages
            .Where(m => (m.SenderId == currentUser.Id && m.ReceiverId == userId) ||
                       (m.SenderId == userId && m.ReceiverId == currentUser.Id))
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .ToListAsync();

        var result = messages.Select(m => new
        {
            id = m.Id,
            content = m.Content,
            mediaUrl = m.MediaUrl,
            sentAt = m.SentAt,
            isRead = m.IsRead,
            readAt = m.ReadAt,
            sender = new
            {
                id = m.Sender.Id,
                displayName = m.Sender.DisplayName,
                profileImageUrl = m.Sender.ProfileImageUrl
            },
            receiver = new
            {
                id = m.Receiver.Id,
                displayName = m.Receiver.DisplayName,
                profileImageUrl = m.Receiver.ProfileImageUrl
            }
        });

        return Ok(result);
    }

    [HttpPut("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(int messageId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == currentUser.Id);

        if (message == null) return NotFound();

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var conversations = await _context.Messages
            .Where(m => m.SenderId == currentUser.Id || m.ReceiverId == currentUser.Id)
            .GroupBy(m => m.SenderId == currentUser.Id ? m.ReceiverId : m.SenderId)
            .Select(g => new
            {
                userId = g.Key,
                lastMessage = g.OrderByDescending(m => m.SentAt).First(),
                unreadCount = g.Count(m => m.ReceiverId == currentUser.Id && !m.IsRead)
            })
            .ToListAsync();

        var userIds = conversations.Select(c => c.userId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var result = conversations.Select(c => new
        {
            user = users.ContainsKey(c.userId) ? new
            {
                id = users[c.userId].Id,
                displayName = users[c.userId].DisplayName,
                userName = users[c.userId].UserName,
                profileImageUrl = users[c.userId].ProfileImageUrl
            } : null,
            lastMessage = new
            {
                id = c.lastMessage.Id,
                content = c.lastMessage.Content,
                sentAt = c.lastMessage.SentAt,
                isFromCurrentUser = c.lastMessage.SenderId == currentUser.Id
            },
            unreadCount = c.unreadCount
        }).Where(c => c.user != null);

        return Ok(result);
    }
}

public class SendMessageRequest
{
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
}
