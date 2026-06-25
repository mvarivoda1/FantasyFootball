using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// MCP server koji izlaže read-only alate nad FantasyFootball web API-jem.
// Pristupa se kroz agentic IDE (Claude Code / Cursor) preko stdio transporta.
var builder = Host.CreateApplicationBuilder(args);

// Stdio transport koristi stdout za MCP protokol → SVI logovi moraju ići na stderr,
// inače bi pokvarili JSON-RPC poruke.
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Bazni URL FantasyFootball web aplikacije (HTTP profil iz launchSettings).
// Override preko konfiguracije "FantasyFootball:BaseUrl" ili env varijable FF_BASE_URL.
var baseUrl = builder.Configuration["FantasyFootball:BaseUrl"]
    ?? Environment.GetEnvironmentVariable("FF_BASE_URL")
    ?? "http://localhost:5263";

builder.Services.AddHttpClient("ff", client => client.BaseAddress = new Uri(baseUrl));

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
