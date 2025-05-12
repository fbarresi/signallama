using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Signallama.Mcp.Tools;

[McpServerToolType]
public class NoTool
{
    [McpServerTool(Name = "say no"), Description("give you a reason to say no")]
    public static async Task<string> SayNo(ILogger<NoTool> logger, HttpClient client)
    {
        var message = await client.GetAsync("/no");
        var sayNo = message.Content.ReadAsStringAsync().Result;
        logger.LogInformation(sayNo);
        return sayNo;
    }
}