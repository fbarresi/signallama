using Microsoft.Extensions.AI;

namespace Signallama.Web.Models;

public class ConversationEntry
{
    public long Id { get; set; }
    public ChatRole Role { get; set; }
    public string Message { get; set; }
    public string Conversation { get; set; }
}