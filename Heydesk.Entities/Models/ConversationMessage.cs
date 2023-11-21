using System;
using System.Collections.Generic;

namespace Heydesk.Entities.Models;

public partial class ConversationMessage
{
    public long Id { get; set; }

    public long ConversationId { get; set; }

    public long SenderId { get; set; }

    public long RecipientId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;

    public virtual User Recipient { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
