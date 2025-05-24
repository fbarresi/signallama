using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol.Types;
using Signallama.Interfaces.HubClients;
using Signallama.Web.Models;
using Signallama.Web.Services;

namespace Signallama.Web.Hubs;

public class ChatHub : Hub<IWebClient>
{
    //see: https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-9.0#inject-services-into-a-hub
    public async Task Send(string conversationId, string message, IChatClient client, ChatOptions options, DocumentStore documentStore)
    {
        var opt = options.Clone();
        opt.ConversationId = conversationId;

        var collection = Context.ConnectionId;
        var messages = documentStore
            .GetCollection<ConversationEntry>(collection, e => e.Conversation == conversationId)
            .OrderBy(e => e.Id)
            .Select(e => new ChatMessage(e.Role, e.Message))
            .ToList();
        messages.Add(new ChatMessage(ChatRole.User, message));
        
        var response = await client.GetResponseAsync(messages, options);
        // var response = await client.GetResponseAsync(message, options);
        var regex = new Regex(@"\<think\>[\s\S]*\<\/think\>");
        var reply = regex.Replace(response.Text, string.Empty).Trim();
        await Clients.Caller.ShowReply(reply);
        documentStore.InsertOrUpdateIntoCollection(collection, new ConversationEntry { Message = message, Role = ChatRole.User, Conversation = conversationId });
        documentStore.InsertOrUpdateIntoCollection(collection, new ConversationEntry { Message = reply, Role = ChatRole.Assistant, Conversation = conversationId});
        
    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        DocumentStore.DropCollection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}