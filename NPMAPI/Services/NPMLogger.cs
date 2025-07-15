using NLog;
using NPMAPI.Repositories;

namespace NPMAPI.Services
{
    public class NPMLogger : INPMLogger
    {
        private static NPMLogger npmLogger;
        private static Logger logger;

        private NPMLogger()
        {

        }
        public static NPMLogger GetInstance()
        {
            if (npmLogger == null)
                npmLogger = new NPMLogger();
            return npmLogger;
        }

        private Logger GetLogger(string theLogger)
        {
            if (NPMLogger.logger == null)
                NPMLogger.logger = LogManager.GetLogger(theLogger);
            return NPMLogger.logger;
        }

        public void Debug(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("npmLoggerRules").Debug(message);
            else
                GetLogger("npmLoggerRules").Debug(message, arg);
        }

        public void Error(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("npmLoggerRules").Error(message);
            else
                GetLogger("npmLoggerRules").Error(message, arg);
        }

        public void Info(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("npmLoggerRules").Info(message);
            else
                GetLogger("npmLoggerRules").Info(message, arg);
        }

        public void Warning(string message, string arg = null)
        {
            if (arg == null)
                GetLogger("npmLoggerRules").Warn(message);
            else
                GetLogger("npmLoggerRules").Warn(message, arg);
        }
    }
}