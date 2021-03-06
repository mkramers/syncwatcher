﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common.Properties;
using log4net;
using log4net.Appender;

namespace Common.Logging
{
    public enum LogLevel
    {
        DEBUG = 0,
        ERROR = 1,
        FATAL = 2,
        INFO = 3,
        WARNING = 4
    }

    /// <summary>
    ///     Write out messages using the logging provider.
    /// </summary>
    public static class Log
    {
        /// <summary>
        ///     Get the <see cref="Logging.NotifyAppender" /> log.
        /// </summary>
        /// <returns>
        ///     The instance of the <see cref="Logging.NotifyAppender" /> log, if configured.
        ///     Null otherwise.
        /// </returns>
        public static NotifyAppender NotifyAppender
        {
            get
            {
                foreach (ILog log in LogManager.GetCurrentLoggers())
                    foreach (IAppender appender in log.Logger.Repository.GetAppenders())
                    {
                        if (appender is NotifyAppender)
                        {
                            return appender as NotifyAppender;
                        }
                    }
                return null;
            }
        }

        /// <summary>
        ///     Static instance of the log manager.
        /// </summary>
        static Log()
        {
            //XmlConfigurator.Configure();
            _actions = new Dictionary<LogLevel, Action<string>>();
            _actions.Add(LogLevel.DEBUG, WriteDebug);
            _actions.Add(LogLevel.ERROR, WriteError);
            _actions.Add(LogLevel.FATAL, WriteFatal);
            _actions.Add(LogLevel.INFO, WriteInfo);
            _actions.Add(LogLevel.WARNING, WriteWarning);

            //set default settings for appender
            NotifyAppenderSettings settings = NotifyAppenderSettings.Default;

            NotifyAppender.IsDebugEnabled = settings.IsDebugEnabled;
            NotifyAppender.SettingChanged += Appender_OnSettingChanged;
        }

        private static void Appender_OnSettingChanged(object _sender, EventArgs _e)
        {
            NotifyAppenderSettings settings = NotifyAppenderSettings.Default;
            settings.IsDebugEnabled = NotifyAppender.IsDebugEnabled;
            settings.Save();
        }

        /// <summary>
        ///     Write the message to the appropriate log based on the relevant log level.
        /// </summary>
        /// <param name="level">The log level to be used.</param>
        /// <param name="message">The message to be written.</param>
        /// <exception cref="ArgumentNullException">Thrown if the message is empty.</exception>
        public static void Write(LogLevel level, string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                if (level > LogLevel.WARNING || level < LogLevel.DEBUG)
                {
                    throw new ArgumentOutOfRangeException("level");
                }

                // Now call the appropriate log level message.
                _actions[level](message);
            }
        }

        #region Members

        private static readonly ILog _logger = LogManager.GetLogger(typeof(Log));
        private static readonly Dictionary<LogLevel, Action<string>> _actions;

        #endregion

        #region Action methods

        private static void WriteDebug(string message)
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(message);
            }
        }

        private static void WriteError(string message)
        {
            if (_logger.IsErrorEnabled)
            {
                _logger.Error(message);
            }
        }

        private static void WriteFatal(string message)
        {
            if (_logger.IsFatalEnabled)
            {
                _logger.Fatal(message);
            }
        }

        private static void WriteInfo(string message)
        {
            if (_logger.IsInfoEnabled)
            {
                _logger.Info(message);
            }
        }

        private static void WriteWarning(string message)
        {
            if (_logger.IsWarnEnabled)
            {
                _logger.Warn(message);
            }
        }

        #endregion
    }
}
