using System.Net;
using System.Net.Http.Json;
using LumiaFoundation.Abstractions.ErrorModel;
using LumiaFoundation.Http.Client.Test.TestDoubles;

namespace LumiaFoundation.Http.Client.Test.Exceptions;

public class ApiErrorTests
{
  [Fact]
  public async Task ReadMessageAsync_WhenErrorHasMessage_ReturnsErrorMessage()
  {
    // Arrange
    var handler = new StubHttpMessageHandler((_, _) =>
        new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
          Content = JsonContent.Create(new ErrorDetails { Message = "Invalid request", ExceptionType = "BadRequest" })
        });
    using var client = new HttpClient(handler);
    using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    {
      Content = JsonContent.Create(new ErrorDetails { Message = "Invalid input", ExceptionType = "BadRequest" })
    };

    // Act
    var message = await ApiError.ReadMessageAsync(response, CancellationToken.None);

    // Assert
    Assert.Equal("Invalid input", message);
  }

  [Fact]
  public async Task ReadMessageAsync_WhenErrorMessageIsNull_ReturnsDefaultMessage()
  {
    // Arrange
    using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    {
      Content = JsonContent.Create(new ErrorDetails { Message = null, ExceptionType = "BadRequest" })
    };

    // Act
    var message = await ApiError.ReadMessageAsync(response, CancellationToken.None);

    // Assert
    Assert.Contains("sem corpo de erro reconhecível", message);
  }

  [Fact]
  public async Task ReadMessageAsync_WhenJsonIsInvalid_ReturnsDefaultMessage()
  {
    // Arrange
    using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
    {
      Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json")
    };

    // Act
    var message = await ApiError.ReadMessageAsync(response, CancellationToken.None);

    // Assert
    Assert.Contains("sem corpo de erro reconhecível", message);
  }
}
