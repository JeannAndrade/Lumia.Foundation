using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lumia.Utility.Foundation.Domain.Exceptions
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