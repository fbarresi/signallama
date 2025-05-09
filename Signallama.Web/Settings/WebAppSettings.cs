using Microsoft.Extensions.AI;

namespace Signallama.Web.Settings;

public class WebAppSettings
{
    public string ChatClientAddress { get; set; } = "https://localhost:7170";
    public string ModelId { get; set; } = "qwen3:0.6b";
}