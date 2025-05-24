using Microsoft.Extensions.AI;
using Signallama.Web.Settings;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Serilog;
using Serilog.Core;
using Signallama.Web.Hubs;
using Signallama.Web.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

//Log
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

Log.Information("Starting up!");

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// Fix listening port by adding kestrel config to your settings
// "Kestrel": {
//     "Endpoints": {
//         "Https": {
//             "Url": "https://*:8443",
//             "Certificate": {
//                 "Subject": "my-fancy-cert",
//                 "Store": "MY",
//                 "Location": "LocalMachine",
//                 "AllowInvalid": "true"
//             }
//         }
//     }
// }

// Add settings to the containers
builder.Services.AddOptions<WebAppSettings>()
    .BindConfiguration("WebAppSettings")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<WebAppSettings>>().Value);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHealthChecks();

builder.Services.AddSignalR();

// add AI Services

var settings = builder.Configuration.GetRequiredSection("WebAppSettings").Get<WebAppSettings>() ?? new WebAppSettings();
var client = new ChatClientBuilder(new OllamaChatClient(new Uri(settings.ChatClientAddress), settings.ModelId))
    .UseFunctionInvocation()
    .Build();

builder.Services.AddSingleton(client);

var chatOptions = new ChatOptions
{
    ModelId = settings.ModelId,
};

var tools = new List<AITool>();
var clientCollection = new ClientCollectionService();

try
{
    var mcpSettings = builder.Configuration.GetRequiredSection("McpSettings").Get<McpSettings>() ?? new McpSettings();
    
    foreach (var server in mcpSettings.Sse)
    {
        var sse = new SseClientTransport(server);
    
        var mcpClient = await McpClientFactory.CreateAsync(sse, new McpClientOptions
        {
            Capabilities = new() { Sampling = new() { SamplingHandler = client.CreateSamplingHandler() } },
        });
        clientCollection.Add(mcpClient);
        tools.AddRange(await mcpClient.ListToolsAsync());
        
        foreach (var tool in tools)
        {
            Log.Information("Connected to server {ServerName} with tools: {ToolName}", server.Name, tool.Name);
        }
    }
    
    foreach (var command in mcpSettings.Stdio)
    {
        var stdio = new StdioClientTransport(command);
    
        var mcpClient = await McpClientFactory.CreateAsync(stdio, new McpClientOptions
        {
            Capabilities = new() { Sampling = new() { SamplingHandler = client.CreateSamplingHandler() } },
        });
        clientCollection.Add(mcpClient);
        tools.AddRange(await mcpClient.ListToolsAsync());
        
        foreach (var tool in tools)
        {
            Log.Information("Connected to Stdio {ServerName} with tools: {ToolName}", command.Name, tool.Name);
        }
    }
    
    chatOptions.Tools = [..tools];
}
catch (Exception e)
{
    Log.Error(e, "Error connecting to MCP server");
}

builder.Services.AddSingleton(chatOptions);
builder.Services.AddSingleton(clientCollection);
builder.Services.AddSingleton<DocumentStore>();
builder.Services.AddSingleton<IHostedService, DocumentStore>(
    serviceProvider => serviceProvider.GetService<DocumentStore>());
//

// Add usage over service
builder.Host.UseWindowsService()
            .UseSystemd();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(options => options.AllowAnyOrigin());

// app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/hubs/chat");

app.Run();