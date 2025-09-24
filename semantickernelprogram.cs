// Semantic Kernel Web API (server)
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var geminiModelId = config["Gemini:ModelId"];
var geminiApiKey = config["Gemini:ApiKey"];

if (string.IsNullOrWhiteSpace(geminiModelId) || string.IsNullOrWhiteSpace(geminiApiKey))
{
    Console.WriteLine("❌ Gemini model ID or API key missing in configuration.");
    return;
}

var httpClientHandler = new HttpClientHandler();
var loggingHttpClient = new HttpClient(new LoggingHandler(httpClientHandler));
loggingHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

// Register Gemini LLM in Semantic Kernel
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddGoogleAIGeminiChatCompletion(
    modelId: geminiModelId,
    apiKey: geminiApiKey,
    httpClient: loggingHttpClient
);
kernelBuilder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Trace));

Kernel kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Register kernel + services for DI
builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton(chatCompletionService);

var app = builder.Build();

app.MapGet("/", () => "✅ Semantic Kernel LLM Server is running.");

app.Run();
