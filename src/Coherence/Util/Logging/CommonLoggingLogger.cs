/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Common.Logging;

using Tangosol.Net;

namespace Tangosol.Util.Logging
{
    /// <summary>
    /// Concrete <see cref="LogOutput"/> extension that logs messages using
    /// the Common.Logging logging library.
    /// </summary>
    public class CommonLoggingLogger : LogOutput
    {
        #region Data members

        private static LogLevel[] s_level;
        private ILog m_logger;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a cache of frequently used Common Logging Level
        /// objects.
        /// </summary>
        /// <value>
        /// An array of Common Logging Level objects representing cache of
        /// frequently used logging level values.
        /// </value>
        protected static LogLevel[] Level
        {
            get { return s_level; }
            set { s_level = value; }
        }

        /// <summary>
        /// Gets or sets the underlying Common.Logging Logger used to log all
        /// messages.
        /// </summary>
        /// <value>
        /// Underlying Common.Logging logger used to log all messages.
        /// </value>
        protected virtual ILog Logger
        {
            get { return m_logger; }
            set { m_logger = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Static initializer.
        /// </summary>
        static CommonLoggingLogger()
        {
            InitStatic();
        }

        #endregion

        #region Initialization methods

        /// <summary>
        /// Initialization of Levels supported by underlying logging
        /// mechanism.
        /// </summary>
        protected static void InitStatic()
        {
            LogLevel[] levels = new LogLevel[]
                {
                  LogLevel.Off,   // LEVEL_NONE
                  LogLevel.Debug, // LEVEL_INTERNAL
                  LogLevel.Error, // LEVEL_ERROR
                  LogLevel.Warn,  // LEVEL_WARNING
                  LogLevel.Info,  // LEVEL_INFO
                  LogLevel.Debug, // LEVEL_D4
                  LogLevel.Debug, // LEVEL_D5
                  LogLevel.Debug, // LEVEL_D6
                  LogLevel.Debug, // LEVEL_D7
                  LogLevel.Debug, // LEVEL_D8
                  LogLevel.Debug, // LEVEL_D9
                  LogLevel.All    // LEVEL_ALL
                };

            Level = levels;
        }

        #endregion

        #region LogOutput override methods

        /// <summary>
        /// Configure a newly created LogOutput instance using the supplied
        /// XML configuration.
        /// </summary>
        /// <param name="operationalContext">
        /// Operational context used to configure this LogOutput object.
        /// </param>
        public override void Configure(IOperationalContext operationalContext)
        {
            base.Configure(operationalContext);

            string loggerName = operationalContext.LogName;
            Logger = LogManager.GetLogger(loggerName);
        }

        /// <summary>
        /// Log the given message with the specified log level (specific to
        /// the underlying logging mechanism).
        /// </summary>
        /// <param name="level">
        /// Level of the message.
        /// </param>
        /// <param name="message">
        /// Message to log.
        /// </param>
        protected override void Log(object level, string message)
        {
            LogInternal(level, null, message);
        }

        /// <summary>
        /// Log the given message with the specified log level (specific to
        /// the underlying logging mechanism).
        /// </summary>
        /// <param name="level">
        /// Level of the message.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        protected override void Log(object level, Exception exception)
        {
            LogInternal(level, exception, null);
        }

        /// <summary>
        /// Log the given message with the specified log level (specific to
        /// the underlying logging mechanism).
        /// </summary>
        /// <param name="level">
        /// Level of the message.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        /// <param name="message">
        /// Message to log.
        /// </param>
        protected override void Log(object level, Exception exception, string message)
        {
            LogInternal(level, exception, message);
        }

        /// <summary>
        /// Translate the given Logger level to an equivalent object
        /// appropriate for the underlying logging mechanism.
        /// </summary>
        /// <param name="level">
        /// Logger log level.
        /// </param>
        /// <returns>
        /// Logging level specific to the underlying logging mechanism that
        /// corresponds to the Logger log level.
        /// </returns>
        protected override object TranslateLevel(int level)
        {
            return GetLevel(level + 1);
        }

        /// <summary>
        /// Gets a Level from a cache of frequently used Common Logging Level
        /// objects.
        /// </summary>
        /// <param name="index">
        /// Position to get level from.
        /// </param>
        protected static LogLevel GetLevel(int index)
        {
            return Level[index];
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Log the given message with the specified log level.
        /// </summary>
        /// <param name="level">
        /// Level of the message.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        /// <param name="message">
        /// Message to log.
        /// </param>
        private void LogInternal(object level, Exception exception, string message)
        {
            switch ((LogLevel) level)
            {
                case LogLevel.Debug :
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(message, exception);
                    }
                    break;

                case LogLevel.Error:
                    if (Logger.IsErrorEnabled)
                    {
                        Logger.Error(message, exception);
                    }
                    break;

                case LogLevel.Fatal:
                    if (Logger.IsFatalEnabled)
                    {
                        Logger.Fatal(message, exception);
                    }
                    break;
                case LogLevel.Info:
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info(message, exception);
                    }
                    break;
                case LogLevel.Warn:
                    if (Logger.IsWarnEnabled)
                    {
                        Logger.Warn(message, exception);
                    }
                    break;
                default:
                    Logger.Debug(message, exception);
                    break;
            }
        }

        #endregion
    }
}
