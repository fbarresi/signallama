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
        var opt = options.Clone();
        opt.ConversationId = conversationId;

        var response = await client.GetResponseAsync(message, options);
        var regex = new Regex(@"\<think\>[\s\S]*\<\/think\>");
        var reply = regex.Replace(response.Text, string.Empty).Trim();
        await Clients.Caller.ShowReply(reply);
        
    }
}