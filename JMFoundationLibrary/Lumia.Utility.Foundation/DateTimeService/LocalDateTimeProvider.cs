using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lumia.Utility.Foundation.DateTimeService
{
  public class LocalDateTimeProvider : IDateTimeProvider
  {
    public DateTime GetDateTime()
    {
      return DateTime.Now;
    }
  }
}