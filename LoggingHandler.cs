using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
public class LoggingHandler : DelegatingHandler
{
    public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Request: " + request);
        var response = await base.SendAsync(request, cancellationToken);
        Console.WriteLine("Response: " + response);
        return response;
    }
}