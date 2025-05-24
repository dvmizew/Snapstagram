using Microsoft.AspNetCore.SignalR;

namespace Snapstagram.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;
            
            // Send to specific user
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
            
            // Also send back to sender for confirmation
            await Clients.Caller.SendAsync("MessageSent", receiverId, message);
        }

        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task LeaveRoom(string roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        }
    }
}
