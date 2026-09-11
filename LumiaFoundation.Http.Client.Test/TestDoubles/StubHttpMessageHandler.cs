using System.Net;
using System.Net.Http;

namespace LumiaFoundation.Http.Client.Test.TestDoubles;

internal sealed class StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestContent { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestContent = request.Content?.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
        return Task.FromResult(responseFactory(request, cancellationToken));
    }
}
