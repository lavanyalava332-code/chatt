// MCP Client entry point
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

var mcpServerUrl = "http://localhost:5000"; // Change if needed
var httpClient = new HttpClient { BaseAddress = new Uri(mcpServerUrl) };

// List available tools
var toolsResponse = await httpClient.GetAsync("/tools");
if (toolsResponse.IsSuccessStatusCode)
{
    var toolsJson = await toolsResponse.Content.ReadAsStringAsync();
    Console.WriteLine($"Available tools: {toolsJson}");
}
else
{
    Console.WriteLine("Failed to fetch tools from MCP server.");
}

