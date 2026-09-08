using System.Net;

namespace LumiaFoundation.Http.Client.Exceptions;

public sealed class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
  public HttpStatusCode StatusCode { get; } = statusCode;
}