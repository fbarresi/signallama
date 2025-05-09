using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Signallama.Mcp.Tools;

[McpServerToolType]
public class BottleTool
{
    [McpServerTool(Name = "bottle"), Description("sends a message in a bottle")]
    public static string Reverse(string message)
    {
        return $"bottle: {message}";
    }
}