using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heydesk.Entities.Objects
{
    public class SendMessageData
    {
        public long? ConversationId { get; set; }
        public long? UserId { get; set; }
        public string Message { get; set; }
    }
}
