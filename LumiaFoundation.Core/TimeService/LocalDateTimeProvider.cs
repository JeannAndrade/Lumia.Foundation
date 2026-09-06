namespace LumiaFoundation.Core.TimeService;

  public class LocalDateTimeProvider : IDateTimeProvider
  {
public DateTime GetDateTime()
{
  return DateTime.Now;
}
  }
