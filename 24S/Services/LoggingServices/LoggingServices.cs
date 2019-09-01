namespace _24S
{
    using MetroLog;
    using MetroLog.Targets;
    using System;

    public class LoggingServices
    {
        #region Properties
        public static LoggingServices Instance { get; }

        public static int RetainDays { get; } = 3; // 3 days

        public static bool Enabled { get; set; } = true;
        #endregion

        #region constructors
        static LoggingServices()
        {
            //implement singleton pattern
            Instance = Instance ?? new LoggingServices();

            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Trace, LogLevel.Fatal, new StreamingFileTarget { RetainDays = 3 });
        }
        #endregion

        #region methods
        public void WriteLine<T>(string message, LogLevel loglevel = LogLevel.Trace, Exception exception = null)
        {
            if (Enabled)
            {
                var logger = LogManagerFactory.DefaultLogManager.GetLogger<T>();
                if(loglevel == LogLevel.Trace && logger.IsTraceEnabled)
                {
                    logger.Trace(message, exception);
                }
                else if (loglevel == LogLevel.Debug && logger.IsDebugEnabled)
                {
                    logger.Debug(message, exception);
                }
                else if (loglevel == LogLevel.Error && logger.IsErrorEnabled)
                {
                    logger.Error(message, exception);
                }
                else if (loglevel == LogLevel.Fatal && logger.IsFatalEnabled)
                {
                    logger.Fatal(message, exception);
                }
                else if (loglevel == LogLevel.Info && logger.IsInfoEnabled)
                {
                    logger.Info(message, exception);
                }
                else if (loglevel == LogLevel.Warn && logger.IsWarnEnabled)
                {
                    logger.Warn(message, exception);
                }
            }
        }
        #endregion
    }
}
