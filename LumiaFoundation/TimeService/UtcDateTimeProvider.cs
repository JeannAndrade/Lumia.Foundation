namespace LumiaFoundation.TimeService
{
  public class UtcDateTimeProvider : IDateTimeProvider
  {
    public DateTime GetDateTime()
    {
      return DateTime.UtcNow;
    }
  }
}