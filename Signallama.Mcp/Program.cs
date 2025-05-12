using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using Signallama.Mcp.Tools;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

// Log

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

// Options

// builder.Services.AddOptions<ApplicationSettings>()
//     .BindConfiguration("ApplicationSettings")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();
// builder.Services.AddScoped(resolver => resolver.GetRequiredService<IOptions<ApplicationSettings>>().Value);


// Services

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<BottleTool>()
    .WithTools<NoTool>()
    //add more tools here
    ;

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddControllers()
                .AddNewtonsoftJson();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Signallama MCP API",
        Description = "REST API to Signallama MCP",
        Contact = new OpenApiContact
        {
            Name = "Signallama corporation",
            Email = "info@Signallama.com",
        }
    });
});

builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://naas.isalman.dev/") };
    return client;
});


// allow run as Service

builder.Host.UseWindowsService()
            .UseSystemd()
            ;

//


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
//put your webpage into wwwroot folder for serving a self-hosted webpage
// learn more here: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-9.0
app.UseDefaultFiles();
app.UseStaticFiles();

// in case of static web ui you can simply use allow any origin
app.UseCors(options => options.AllowAnyOrigin());

// app.UseCors(options =>
//     options.AllowAnyHeader()
//         .AllowAnyMethod()
//         .SetIsOriginAllowed((host) => true)
//         .AllowCredentials()
// );

app.MapControllers();
app.MapHealthChecks("/health");

// MCP
app.MapMcp();

app.Run();
