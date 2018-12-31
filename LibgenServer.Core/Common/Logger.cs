using System;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using static LibgenServer.Core.Common.Constants;

namespace LibgenServer.Core.Common
{
    public class Logger
    {
        private static NLog.Logger logger;
        private static bool loggingEnabled;

        static Logger()
        {
            logger = null;
        }

        public static void EnableLogging()
        {
            if (logger == null)
            {
                CreateLogger();
                LogEnvironmentInformation();
            }
            loggingEnabled = true;
        }

        public static void DisableLogging()
        {
            loggingEnabled = false;
        }

        public static void Debug(params string[] lines)
        {
            if (loggingEnabled)
            {
                logger.Log(typeof(Logger), new LogEventInfo(LogLevel.Debug, String.Empty, String.Join("\r\n", lines.Where(line => !String.IsNullOrEmpty(line)))));
            }
        }

        public static void Exception(Exception exception)
        {
            if (loggingEnabled)
            {
                logger.Log(typeof(Logger), new LogEventInfo(LogLevel.Error, String.Empty, exception.ToString()));
            }
        }

        private static void CreateLogger()
        {
            LoggingConfiguration loggingConfiguration = new LoggingConfiguration();
            FileTarget fileTarget = new FileTarget
            {
                FileName = Environment.LogFilePath,
                Layout = "${longdate} [${threadid}] ${callsite} ${level:uppercase=true} ${message}"
            };
            loggingConfiguration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));
            LogManager.Configuration = loggingConfiguration;
            logger = LogManager.GetLogger(String.Empty);
        }

        private static void LogEnvironmentInformation()
        {
            logger.Debug("Libgen Server " + CURRENT_VERSION);
            logger.Debug("Release: " + CURRENT_GITHUB_RELEASE_NAME);
            logger.Debug("OS: " + Environment.OsVersion);
            logger.Debug("Target framework: " + Environment.TargetFramework);
            logger.Debug("Is in 64-bit process: " + Environment.IsIn64BitProcess);
        }
    }
}
