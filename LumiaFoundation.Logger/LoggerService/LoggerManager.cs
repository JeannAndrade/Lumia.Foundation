using LumiaFoundation.Logger.Contracts;
using NLog;

namespace LumiaFoundation.Logger.LoggerService
{
    public class LoggerManager : ILoggerManager
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();

        public void LogDebug(string message) => logger.Debug(message);
        public void LogError(string message) => logger.Error(message);
        public void LogInfo(string message) => logger.Info(message);
        public void LogWarn(string message) => logger.Warn(message);

        public static void LoadConfigurationFromFile(string fullPathConfigurationFile)
        {
            if (File.Exists(fullPathConfigurationFile))
            {
                LogManager.Setup().LoadConfigurationFromFile(fullPathConfigurationFile);
            }
        }
    }


}