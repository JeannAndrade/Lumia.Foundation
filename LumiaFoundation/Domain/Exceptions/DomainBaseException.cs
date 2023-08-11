namespace LumiaFoundation.Domain.Exceptions
{
  public class GenericDomainException : Exception
  {
    public GenericDomainException() : base()
    {
    }

    public GenericDomainException(string message) : base(message)
    {
    }

    public GenericDomainException(string message, Exception inner) : base(message, inner)
    {
    }
  }
}