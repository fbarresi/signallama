using ModelContextProtocol.Client;

namespace Signallama.Web.Services;

public class ClientCollectionService : IAsyncDisposable
{
    public List<IMcpClient> Clients { get; set; } = new List<IMcpClient>();

    public async ValueTask DisposeAsync()
    {
        foreach (var client in Clients)
        {
            await client.DisposeAsync().ConfigureAwait(false);
        }
    }
    
    public void Add(IMcpClient client) => Clients.Add(client);
    public void AddRange(params IMcpClient[] clients) => Clients.AddRange(clients);
}