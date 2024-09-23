namespace LumiaFoundation.AspNetCore.Commons.Exceptions
{
  [Serializable]
  public class DomainBaseException : Exception
  {
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