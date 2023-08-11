namespace LumiaFoundation.TimeService
{
  public class LocalDateTimeProvider : IDateTimeProvider
  {
    public DateTime GetDateTime()
    {
      return DateTime.Now;
    }
  }
}