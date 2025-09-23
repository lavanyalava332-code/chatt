using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Load configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var geminiModelId = config["Gemini:ModelId"];
        var geminiApiKey = config["Gemini:ApiKey"];

        // Create kernel
        var builder = Kernel.CreateBuilder();
        builder.AddGoogleAIGeminiChatCompletion(geminiModelId, geminiApiKey);

        // Add logging
        builder.Services.AddLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Trace));

        Kernel kernel = builder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        // Register your plugin
        kernel.Plugins.AddFromType<LightsPlugin>("Lights");

        // Execution settings for Gemini
        var geminiPromptExecutionSettings = new GeminiPromptExecutionSettings
        {
            FunctionCallBehavior = FunctionCallBehavior.AutoInvokeKernelFunctions
        };

        // Chat loop
        var history = new ChatHistory();
        string? userInput;

        do
        {
            Console.Write("User > ");
            userInput = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userInput)) continue;

            history.AddUserMessage(userInput);

            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: geminiPromptExecutionSettings,
                kernel: kernel);

            Console.WriteLine("Assistant > " + result);
            history.AddMessage(result.Role, result.Content ?? string.Empty);

        } while (userInput is not null);
    }
}
