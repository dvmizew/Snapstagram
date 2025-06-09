using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Snapstagram.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task LeaveConversation(string conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }

        public async Task JoinUserRoom()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
        }

        public async Task LeaveUserRoom()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
        }

        public async Task JoinChatGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chatgroup_{groupId}");
        }

        public async Task LeaveChatGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chatgroup_{groupId}");
        }

        public async Task SendTypingIndicator(string recipientId, bool isTyping)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(recipientId))
            {
                await Clients.Group($"user_{recipientId}")
                    .SendAsync("TypingIndicator", userId, isTyping);
            }
        }

        public async Task SendGroupTypingIndicator(string groupId, bool isTyping)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(groupId))
            {
                await Clients.Group($"chatgroup_{groupId}")
                    .SendAsync("GroupTypingIndicator", userId, isTyping);
            }
        }

        public async Task MarkMessageAsRead(string messageId, string senderId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(senderId))
            {
                await Clients.Group($"user_{senderId}")
                    .SendAsync("MessageRead", messageId, userId);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await JoinUserRoom();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await LeaveUserRoom();
            await base.OnDisconnectedAsync(exception);
        }
    }
}
