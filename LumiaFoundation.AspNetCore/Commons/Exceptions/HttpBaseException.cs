namespace LumiaFoundation.AspNetCore.Commons.Exceptions;

[Serializable]
public class HttpBaseException : Exception
{
  protected int StatusCodeValue { get; }

  public int StatusCode => StatusCodeValue;

  public HttpBaseException(int statusCode) : base()
  {
    StatusCodeValue = statusCode;
  }

  public HttpBaseException(int statusCode, string message) : base(message)
  {
    StatusCodeValue = statusCode;
  }

  public HttpBaseException(int statusCode, string message, Exception inner) : base(message, inner)
  {
    StatusCodeValue = statusCode;
  }
}
