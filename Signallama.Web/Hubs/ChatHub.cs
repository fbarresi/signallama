using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using Signallama.Interfaces.HubClients;

namespace Signallama.Logic.Hubs;

public class ChatHub : Hub<IWebClient>
{
    //see: https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-9.0#inject-services-into-a-hub
    public async Task Send(string conversationId, string message, IChatClient client, ChatOptions options)
    {
        options.ConversationId = conversationId;
        await foreach (var update in client.GetStreamingResponseAsync(message, options))
        {
            var regex = new Regex(@"\<think\>[\s\S]*\<\/think\>");
            var reply = regex.Replace(update.Text, string.Empty).Trim();
            await Clients.Caller.ShowReply(reply);
        }
        
    }
}