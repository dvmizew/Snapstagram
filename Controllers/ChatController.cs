using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Snapstagram.Data;
using Snapstagram.Hubs;
using Snapstagram.Models;
using Snapstagram.Services;
using System.Security.Claims;

namespace Snapstagram.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatController(ApplicationDbContext context, NotificationService notificationService, IHubContext<ChatHub> chatHubContext)
        {
            _context = context;
            _notificationService = notificationService;
            _chatHubContext = chatHubContext;
        }

        [HttpGet("StartConversation")]
        public async Task<IActionResult> StartConversation(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null || currentUserId == userId)
            {
                return RedirectToPage("/Chat/Index");
            }

            // Check if conversation already exists
            var existingMessage = await _context.Messages
                .FirstOrDefaultAsync(m => 
                    (m.SenderId == currentUserId && m.RecipientId == userId) ||
                    (m.SenderId == userId && m.RecipientId == currentUserId));

            if (existingMessage != null)
            {
                return RedirectToPage("/Chat/Conversation", new { userId = userId });
            }

            // If no conversation exists, redirect to conversation page anyway (will be empty)
            return RedirectToPage("/Chat/Conversation", new { userId = userId });
        }

        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromForm] CreateGroupRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, message = "Group name is required" });
            }

            if (request.MemberIds == null || !request.MemberIds.Any())
            {
                return Json(new { success = false, message = "At least one member must be selected" });
            }

            try
            {
                // Create the group
                var group = new ChatGroup
                {
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    CreatedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ChatGroups.Add(group);
                await _context.SaveChangesAsync();

                // Add the creator as owner
                var creatorMember = new ChatGroupMember
                {
                    ChatGroupId = group.Id,
                    UserId = currentUserId,
                    Role = GroupMemberRole.Owner,
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ChatGroupMembers.Add(creatorMember);

                // Add selected members
                foreach (var memberId in request.MemberIds)
                {
                    if (memberId != currentUserId) // Don't add creator twice
                    {
                        var member = new ChatGroupMember
                        {
                            ChatGroupId = group.Id,
                            UserId = memberId,
                            Role = GroupMemberRole.Member,
                            JoinedAt = DateTime.UtcNow,
                            AddedByUserId = currentUserId,
                            IsActive = true
                        };

                        _context.ChatGroupMembers.Add(member);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, groupId = group.Id });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while creating the group" });
            }
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageRequest request)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Json(new { success = false, message = "Message content is required" });
            }

            try
            {
                if (!string.IsNullOrEmpty(request.RecipientId))
                {
                    // Individual message
                    var message = new Message
                    {
                        SenderId = currentUserId,
                        RecipientId = request.RecipientId,
                        Content = request.Content.Trim(),
                        SentAt = DateTime.UtcNow,
                        IsRead = false,
                        IsDeleted = false
                    };

                    _context.Messages.Add(message);
                    await _context.SaveChangesAsync();

                    // Get sender information for real-time broadcast
                    var sender = await _context.Users.FindAsync(currentUserId);
                    
                    // Broadcast message in real-time via SignalR
                    if (sender != null)
                    {
                        var messageData = new
                        {
                            id = message.Id,
                            senderId = currentUserId,
                            senderName = $"{sender.FirstName} {sender.LastName}",
                            senderAvatar = sender.ProfilePictureUrl ?? "/images/default-avatar.png",
                            content = message.Content,
                            sentAt = message.SentAt,
                            isRead = false
                        };

                        // Send to both users in the conversation
                        await _chatHubContext.Clients.Group($"user_{currentUserId}")
                            .SendAsync("ReceiveMessage", messageData);
                        await _chatHubContext.Clients.Group($"user_{request.RecipientId}")
                            .SendAsync("ReceiveMessage", messageData);
                    }

                    // Send notification to the recipient
                    try
                    {
                        if (sender != null && !string.IsNullOrEmpty(request.RecipientId))
                        {
                            await _notificationService.SendNotificationAsync(
                                request.RecipientId,
                                $"{sender.FirstName} {sender.LastName} sent you a message",
                                NotificationType.NewMessage,
                                message.Id.ToString(),
                                $"{sender.FirstName} {sender.LastName}"
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the message sending
                        Console.WriteLine($"Failed to send notification: {ex.Message}");
                    }

                    return Json(new { success = true, messageId = message.Id });
                }
                else if (request.GroupId.HasValue)
                {
                    // Group message
                    // Verify user is member of the group
                    var membership = await _context.ChatGroupMembers
                        .FirstOrDefaultAsync(m => m.ChatGroupId == request.GroupId && m.UserId == currentUserId && m.IsActive);

                    if (membership == null)
                    {
                        return Json(new { success = false, message = "You are not a member of this group" });
                    }

                    var groupMessage = new GroupMessage
                    {
                        ChatGroupId = request.GroupId.Value,
                        SenderId = currentUserId,
                        Content = request.Content.Trim(),
                        SentAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _context.GroupMessages.Add(groupMessage);
                    await _context.SaveChangesAsync();

                    // Get sender and group information for real-time broadcast
                    var sender = await _context.Users.FindAsync(currentUserId);
                    var group = await _context.ChatGroups.FindAsync(request.GroupId.Value);
                    
                    // Broadcast group message in real-time via SignalR
                    if (sender != null && group != null)
                    {
                        var messageData = new
                        {
                            id = groupMessage.Id,
                            senderId = currentUserId,
                            senderName = $"{sender.FirstName} {sender.LastName}",
                            senderAvatar = sender.ProfilePictureUrl ?? "/images/default-avatar.png",
                            content = groupMessage.Content,
                            sentAt = groupMessage.SentAt,
                            groupId = request.GroupId.Value,
                            groupName = group.Name
                        };

                        // Send to all group members
                        await _chatHubContext.Clients.Group($"chatgroup_{request.GroupId.Value}")
                            .SendAsync("ReceiveGroupMessage", messageData);

                        // Send notifications to all group members (except sender)
                        var groupMembers = await _context.ChatGroupMembers
                            .Where(m => m.ChatGroupId == request.GroupId.Value && m.IsActive && m.UserId != currentUserId)
                            .ToListAsync();

                        foreach (var member in groupMembers)
                        {
                            if (!string.IsNullOrEmpty(member.UserId))
                            {
                                await _notificationService.SendNotificationAsync(
                                    member.UserId,
                                    $"{sender.FirstName} {sender.LastName} sent a message in '{group.Name}'",
                                    NotificationType.NewGroupMessage,
                                    groupMessage.Id.ToString(),
                                    $"{sender.FirstName} {sender.LastName}"
                                );
                            }
                        }
                    }

                    return Json(new { success = true, messageId = groupMessage.Id });
                }
                else
                {
                    return Json(new { success = false, message = "Invalid message target" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while sending the message" });
            }
        }

        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead([FromForm] int messageId, [FromForm] bool isGroup = false)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Json(new { success = false, message = "User not authenticated" });
            }

            try
            {
                if (isGroup)
                {
                    // Check if already marked as read
                    var existingRead = await _context.GroupMessageReads
                        .FirstOrDefaultAsync(r => r.GroupMessageId == messageId && r.UserId == currentUserId);

                    if (existingRead == null)
                    {
                        var readStatus = new GroupMessageRead
                        {
                            GroupMessageId = messageId,
                            UserId = currentUserId,
                            ReadAt = DateTime.UtcNow
                        };

                        _context.GroupMessageReads.Add(readStatus);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var message = await _context.Messages
                        .FirstOrDefaultAsync(m => m.Id == messageId && m.RecipientId == currentUserId);

                    if (message != null)
                    {
                        message.IsRead = true;
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred" });
            }
        }
    }

    public class CreateGroupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> MemberIds { get; set; } = new();
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? RecipientId { get; set; }
        public int? GroupId { get; set; }
    }
}
