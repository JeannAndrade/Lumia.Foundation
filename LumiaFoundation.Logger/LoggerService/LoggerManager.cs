using LumiaFoundation.Logger.Contracts;
using NLog;

namespace LumiaFoundation.Logger.LoggerService
{
    public class LoggerManager : ILoggerManager
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public void LogDebug(string message) => logger.Debug(message);
        public void LogDebug(string message, params object[] objects) => logger.Debug(message, objects);
        public void LogWarn(string message) => logger.Warn(message);
        public void LogWarn(string message, params object[] objects) => logger.Warn(message, objects);
        public void LogInfo(string message) => logger.Info(message);
        public void LogInfo(string message, params object[] objects) => logger.Info(message, objects);
        public void LogError(string message) => logger.Error(message);
        public void LogError(Exception exception, string message) => logger.Error(exception, message);
        public void LogError(Exception exception, string message, params object[] objects) => logger.Error(exception, message, objects);

        public static void LoadConfigurationFromFile(string fullPathConfigurationFile)
        {
            if (File.Exists(fullPathConfigurationFile))
            {
                LogManager.Setup().LoadConfigurationFromFile(fullPathConfigurationFile);
            }
        }
    }


}