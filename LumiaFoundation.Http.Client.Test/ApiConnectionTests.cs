using System.Net;
using System.Net.Http.Json;
using LumiaFoundation.Http.Client.Exceptions;
using LumiaFoundation.Http.Client.Test.TestDoubles;

namespace LumiaFoundation.Http.Client.Test;

public class ApiConnectionTests
{
    [Fact]
    public async Task SendAsyncOfT_WhenResponseContainsJson_ReturnsDeserializedValue()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new WeatherResponse("sunny"))
        });
        var connection = new ApiConnection(CreateHttpClient(handler));

        // Act
        var result = await connection.SendAsync<WeatherResponse>(HttpMethod.Get, "/weather");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("sunny", result.Condition);
    }

    [Fact]
    public async Task SendAsyncOfT_WhenResponseHasEmptyContent_ReturnsDefault()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.NoContent));
        var connection = new ApiConnection(CreateHttpClient(handler));

        // Act
        var result = await connection.SendAsync<WeatherResponse>(HttpMethod.Get, "/weather");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SendAsync_WhenBodyIsProvided_SendsJsonRequestAndCompletes()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.Accepted));
        var connection = new ApiConnection(CreateHttpClient(handler));
        var body = new WeatherRequest("Sao Paulo");

        // Act
        await connection.SendAsync(HttpMethod.Post, "/weather", body);

        // Assert
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/weather", handler.LastRequest.RequestUri!.PathAndQuery);
        Assert.Equal("application/json", handler.LastRequest.Content!.Headers.ContentType!.MediaType);
        Assert.Contains("Sao Paulo", handler.LastRequestContent);
    }

    [Fact]
    public async Task SendAsync_WhenApiReturnsStructuredError_ThrowsApiExceptionWithApiMessage()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "Invalid city",
                exceptionType = "ValidationException"
            })
        });
        var connection = new ApiConnection(CreateHttpClient(handler));

        // Act
        var exception = await Assert.ThrowsAsync<ApiException>(() => connection.SendAsync(HttpMethod.Get, "/weather"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("Invalid city", exception.Message);
    }

    [Fact]
    public async Task SendAsync_WhenApiReturnsUnrecognizedError_ThrowsApiExceptionWithFallbackMessage()
    {
        // Arrange
        var handler = new StubHttpMessageHandler((_, _) => new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("<html>proxy error</html>")
        });
        var connection = new ApiConnection(CreateHttpClient(handler));

        // Act
        var exception = await Assert.ThrowsAsync<ApiException>(() => connection.SendAsync(HttpMethod.Get, "/weather"));

        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
        Assert.Equal("A API retornou 502 sem corpo de erro reconhecível.", exception.Message);
    }

    private sealed record WeatherResponse(string Condition);
    private sealed record WeatherRequest(string City);

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        => new(handler) { BaseAddress = new Uri("https://api.lumia.test/") };
}
