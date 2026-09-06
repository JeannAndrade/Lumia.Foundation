
namespace LumiaFoundation.Core.Domain.Exceptions;

[Serializable]
public abstract class DomainBaseException : Exception
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