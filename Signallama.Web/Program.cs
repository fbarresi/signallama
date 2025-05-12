using Microsoft.Extensions.AI;
using Signallama.Web.Settings;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using Serilog;
using Serilog.Core;
using Signallama.Logic.Hubs;

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

try
{
    var sse = new SseClientTransport(new()
    {
        Endpoint = new Uri("https://localhost:7170"),
        UseStreamableHttp = true,
        Name = "MyServer",
        ConnectionTimeout = TimeSpan.FromSeconds(10),
        AdditionalHeaders = null
    });

    await using var mcpClient = await McpClientFactory.CreateAsync(sse, new McpClientOptions
    {
        Capabilities = new() { Sampling = new() { SamplingHandler = client.CreateSamplingHandler() } },
    });

    var tools = await mcpClient.ListToolsAsync();
    foreach (var tool in tools)
    {
        Log.Information($"Connected to server with tools: {tool.Name}");
    }
    chatOptions.Tools = [..tools];
}
catch (Exception e)
{
    Log.Error(e, "Error connecting to MCP server");
}


builder.Services.AddSingleton(chatOptions);
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