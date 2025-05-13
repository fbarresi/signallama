using System.Collections;
using ModelContextProtocol.Protocol.Transport;

namespace Signallama.Web.Settings;

public class McpSettings
{
    public SseClientTransportOptions[] Sse { get; set; } = [];
    public StdioClientTransportOptions[] Stdio { get; set; } = [];
}