namespace LumiaFoundation.AspNetCore.Commons.Exceptions
{
  [Serializable]
  public class DomainBaseException : Exception
  {
    private readonly int _statusCode;

    public int StatusCode => _statusCode;

    public DomainBaseException(int statusCode) : base()
    {
      _statusCode = statusCode;
    }

    public DomainBaseException(int statusCode, string message) : base(message)
    {
      _statusCode = statusCode;
    }

    public DomainBaseException(int statusCode, string message, Exception inner) : base(message, inner)
    {
      _statusCode = statusCode;
    }


  }
}