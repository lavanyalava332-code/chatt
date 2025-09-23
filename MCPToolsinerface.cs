namespace Chatbot.Plugins
{
    public interface ITool
    {
        Task<string?> CheckAsync(string userId, string issue);
    }
}
