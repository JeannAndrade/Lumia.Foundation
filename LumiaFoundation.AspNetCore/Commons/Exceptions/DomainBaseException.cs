namespace LumiaFoundation.AspNetCore.Commons.Exceptions
{
  [Serializable]
  public abstract class DomainBaseException : Exception
  {
    protected abstract int StatusCodeValue { get; }

    public int StatusCode => StatusCodeValue;

    public DomainBaseException() : base()
    {
    }

    public DomainBaseException(string message) : base(message)
    {
    }

    public DomainBaseException(string message, Exception inner) : base(message, inner)
    {
    }
  }
}