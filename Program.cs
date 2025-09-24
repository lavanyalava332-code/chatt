// Semantic Kernel Web API
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var geminiModelId = config["Gemini:ModelId"];
var geminiApiKey = config["Gemini:ApiKey"];
var mcpServerUrl = config["McpServer:Url"] ?? "http://localhost:5000";

if (string.IsNullOrWhiteSpace(geminiModelId) || string.IsNullOrWhiteSpace(geminiApiKey))
{
	Console.WriteLine("❌ Gemini model ID or API key missing in configuration.");
	return;
}

var httpClientHandler = new HttpClientHandler();
var loggingHttpClient = new HttpClient(new LoggingHandler(httpClientHandler));
loggingHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddGoogleAIGeminiChatCompletion(
    modelId: geminiModelId,
    apiKey: geminiApiKey,
    httpClient: loggingHttpClient
);
kernelBuilder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Trace));
Kernel kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var builderWeb = WebApplication.CreateBuilder(args);
builderWeb.Services.AddSingleton(kernel);
builderWeb.Services.AddSingleton(chatCompletionService);
var app = builderWeb.Build();

// ✅ Memory store (simple in-memory for demo)
var memoryStore = new Dictionary<string, string>();

// Interactive chat loop for Semantic Kernel
app.MapPost("/chat", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var userInput = await reader.ReadToEndAsync();

    var history = new ChatHistory();
    history.AddSystemMessage("You are a friendly assistant who helps users manage bulbs. You have access to a plugin called \"DatabasePlugin\" with a function called get_lights. Use it when the user asks about bulbs or lights. Respond in the user's language and keep it conversational.");

    if (string.IsNullOrWhiteSpace(userInput))
    {
        history.AddUserMessage(" "); // Trigger LLM to start the conversation
    }
    else
    {
        history.AddUserMessage(userInput);
        memoryStore["lastQuery"] = userInput;
    }

    try
    {
        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            kernel: kernel
        );
        await context.Response.WriteAsync(result.Content ?? "🤖 No response.");
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"Error: {ex.Message}");
    }
});


// Console chat loop for direct user interaction
while (true)
{
    Console.WriteLine("Enter your query for the LLM (or 'exit' to quit):");
    var query = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(query) || query.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    var history = new ChatHistory();
    history.AddSystemMessage("You are a friendly assistant who helps users manage bulbs. You have access to a plugin called \"DatabasePlugin\" with a function called get_lights. Use it when the user asks about bulbs or lights. Respond in the user's language and keep it conversational.");
    history.AddUserMessage(query);

    try
    {
        var result = await chatCompletionService.GetChatMessageContentAsync(
            history,
            kernel: kernel
        );
        Console.WriteLine($"LLM response: {result.Content ?? "🤖 No response."}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}
app.Run();