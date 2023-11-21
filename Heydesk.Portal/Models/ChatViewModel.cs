using Heydesk.Entities.Models;

namespace Heydesk.Portal.Models
{
    public class ChatViewModel : BaseModel
    {
        public ConversationMessage? ConversationMessage { get; set; }
        public Conversation? Conversation { get; set; }
        public List<Conversation>? Conversations { get; set; }
    }
}
