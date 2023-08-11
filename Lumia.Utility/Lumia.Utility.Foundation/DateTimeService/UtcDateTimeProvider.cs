namespace Lumia.Utility.Foundation.DateTimeService
{
  public class UtcDateTimeProvider : IDateTimeProvider
  {
    public DateTime GetDateTime()
    {
      return DateTime.UtcNow;
    }
  }
}