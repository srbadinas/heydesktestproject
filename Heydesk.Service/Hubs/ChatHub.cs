using Heydesk.Entities.Objects;
using Microsoft.AspNetCore.SignalR;

namespace Heydesk.Service.Hubs
{
    public class ChatHub : Hub
    {
        public async Task ConnectChat(long? conversationId) {
            if (conversationId.HasValue) {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Conversation-" + conversationId);

            }
        }

        public async Task DisconnectChat(long? conversationId)
        {
            if (conversationId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Conversation-" + conversationId);
            }
        }

        public async Task SendMessage(long? conversationId, string message, bool isMe)
        {
            Clients.Group("Conversation-" + conversationId).SendAsync("ReceiveMessage", message, isMe);
        }
    }
}
