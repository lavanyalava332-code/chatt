// MCP Server entry point
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer().WithToolsFromAssembly();

// Register DB connection string as singleton
builder.Services.AddSingleton<string>(_ =>
    Environment.GetEnvironmentVariable("LIGHTBULBS_DB_CONNECTION") ?? "Host=localhost;Username=postgres;Password=postgres;Database=lightbulbs"
);

var app = builder.Build();

// Example HTTP endpoints
app.MapGet("/lights", () => LightbulbTools.GetLights());
app.MapPost("/lights/{id}/state", (int id, [FromBody] bool isOn) => LightbulbTools.ChangeState(id, isOn));

// Add /tools endpoint to list available MCP tools
app.MapGet("/tools", () => new[] { "GetLights", "ChangeState" });

// Add /mcp endpoint for MCP Inspector Streamable HTTP
app.MapPost("/mcp", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var requestBody = await reader.ReadToEndAsync();
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(requestBody);
});


await app.RunAsync();