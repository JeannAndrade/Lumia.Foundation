using LumiaFoundation.Logger.Contracts;

namespace LumiaFoundation.AspNetCore.Test.TestDoubles;

internal sealed class FakeLoggerManager : ILoggerManager
{
    public List<string> DebugMessages { get; } = [];
    public List<string> InfoMessages { get; } = [];
    public List<string> WarnMessages { get; } = [];
    public List<string> ErrorMessages { get; } = [];
    public List<Exception> Exceptions { get; } = [];

    public void LogDebug(string message) => DebugMessages.Add(message);
    public void LogDebug(string message, params object[] objects) => DebugMessages.Add(string.Format(message, objects));
    public void LogWarn(string message) => WarnMessages.Add(message);
    public void LogWarn(string message, params object[] objects) => WarnMessages.Add(string.Format(message, objects));
    public void LogWarn(Exception exception, string message)
    {
        Exceptions.Add(exception);
        WarnMessages.Add(message);
    }
    public void LogInfo(string message) => InfoMessages.Add(message);
    public void LogInfo(string message, params object[] objects) => InfoMessages.Add(string.Format(message, objects));
    public void LogError(string message) => ErrorMessages.Add(message);
    public void LogError(Exception exception, string message)
    {
        Exceptions.Add(exception);
        ErrorMessages.Add(message);
    }
    public void LogError(Exception exception, string message, params object[] objects)
    {
        Exceptions.Add(exception);
        ErrorMessages.Add(string.Format(message, objects));
    }
}
