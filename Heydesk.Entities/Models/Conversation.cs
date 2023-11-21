using System;
using System.Collections.Generic;

namespace Heydesk.Entities.Models;

public partial class Conversation
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public bool IsEnded { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool? IsAiResponse { get; set; }

    public virtual ICollection<ConversationMessage> ConversationMessages { get; set; } = new List<ConversationMessage>();

    public virtual User User { get; set; } = null!;
}
