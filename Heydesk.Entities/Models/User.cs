using System;
using System.Collections.Generic;

namespace Heydesk.Entities.Models;

public partial class User
{
    public long Id { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public bool IsOperator { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<ConversationMessage> ConversationMessageRecipients { get; set; } = new List<ConversationMessage>();

    public virtual ICollection<ConversationMessage> ConversationMessageSenders { get; set; } = new List<ConversationMessage>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}
