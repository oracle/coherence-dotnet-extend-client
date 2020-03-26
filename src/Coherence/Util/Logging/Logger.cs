/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Reflection;
using System.Threading;

using Tangosol.Net;
using Tangosol.Run.Xml;
using Tangosol.Util.Daemon.QueueProcessor;

namespace Tangosol.Util.Logging
{
    /// <summary>
    /// A Logger class is used to to asynchronously log messages for a
    /// specific system or application component.
    /// </summary>
    /// <remarks>
    /// <p/>
    /// Each Logger instance has an associated logging level. Only Log
    /// messages that meet or exceed this level are logged. Currently, the
    /// Logger defines 10 logging levels (from highest to lowest level):
    ///
    /// LEVEL_INTERNAL (All messages without a Log level)
    /// LEVEL_ERROR    (Error messages)
    /// LEVEL_WARNING  (Warning messages)
    /// LEVEL_INFO     (Informational messages)
    /// LEVEL_D4       (Debug messages)
    /// LEVEL_D5
    /// LEVEL_D6
    /// LEVEL_D7
    /// LEVEL_D8
    /// LEVEL_D9
    ///
    /// Additionally, the Logger defines two "psuedo" levels that instruct
    /// the Logger to either log all messages or to not log any messages:
    ///
    /// LEVEL_ALL
    /// LEVEL_NONE
    ///
    /// Log messages are logged using the Log() method. There are several
    /// versions of the Log() method that allow both string messages and
    /// Exception stack traces to be logged. The Logger uses a string
    /// template to format the Log message before it is logged using the
    /// underlying logging mechanism. The template may contain the following
    /// parameterizable strings:
    ///
    /// {date}    -the date and time that the message was logged
    /// {level}   -the level of the Log message
    /// {thread}  -the thread that logged the message
    /// {text}    -the text of the message
    /// {product} -the executing assembly product
    /// {version} -the executing assembly version
    ///
    /// Subclasses of the Logger are free to define additional parameters.
    ///
    /// The Logger class uses a <see cref="LogOutput"/> to log messages to an
    /// underlying logging mechanism, such as Out, Error, a file. See the
    /// Configure() method for additional detail.
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.09.19</author>
    public class Logger : QueueProcessor
    {
        #region Properties

        /// <summary>
        /// Gets or sets a cache of frequently used Integer objects that
        /// represent logging levels.
        /// </summary>
        /// <value>
        /// An array of integers representing cache of frequently used
        /// logging levels.
        /// </value>
        protected static int[] Integer
        {
            get { return s_integer; }
            set { s_integer = value; }
        }

        /// <summary>
        /// Gets the virtual constant DefaultDestination.
        /// </summary>
        /// <value>
        /// The default logging destination.
        /// </value>
        public static  string DefaultDestination
        {
            get { return "stderr"; }
        }

        /// <summary>
        /// Gets the virtual constant DefaultFormat.
        /// </summary>
        /// <value>
        /// Default log message format template.
        /// </value>
        public static string DefaultFormat
        {
            get { return "{date} {product} {version} &lt;{level}&gt; (thread={thread}): {text}"; }
        }

        /// <summary>
        /// Gets the virtual constant DefaultLevel.
        /// </summary>
        /// <value>
        /// Default logging level.
        /// </value>
        public static int DefaultLevel
        {
            get { return 10; }
        }

        /// <summary>
        /// Gets the virtual constant DefaultLimit.
        /// </summary>
        /// <value>
        /// The default logging character limit.
        /// </value>
        public static int DefaultLimit
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the virtual constant DefaultName.
        /// </summary>
        /// <value>
        /// The default logger name.
        /// </value>
        public static string DefaultName
        {
            get { return "Coherence"; }
        }

        /// <summary>
        /// Gets the virtual constant DefaultParameters.
        /// </summary>
        /// <value>
        /// An array of default parameterizable strings that may appear in
        /// formatted log messages.
        /// </value>
        public virtual string[] DefaultParameters
        {
            get { return (string[]) s_defaultParameters.Clone(); }
        }

        /// <summary>
        /// Gets or sets the Log message format template.
        /// </summary>
        /// <value>
        /// Log message format template.
        /// </value>
        public virtual string Format
        {
            get { return m_format; }
            set { m_format = value; }
        }

        /// <summary>
        /// Gets or sets the logging level.
        /// </summary>
        /// <value>
        /// Logging level.
        /// </value>
        public virtual int Level
        {
            get { return m_level; }
            set { m_level = value; }
        }

        /// <summary>
        /// Gets or sets the logging character limit.
        /// </summary>
        /// <value>
        /// The logging character limit.
        /// </value>
        public virtual int Limit
        {
            get { return m_limit; }
            set { m_limit = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="LogOutput"/> used to log all
        /// formatted Log messages.
        /// </summary>
        /// <value>
        /// <b>LogOutput</b> used to log all log messages.
        /// </value>
        protected virtual LogOutput LogOutput
        {
            get { return m_logOutput; }
            set { m_logOutput = value; }
        }

        /// <summary>
        /// Gets or sets the set of parameterizable strings that may appear
        /// in formatted Log messages.
        /// </summary>
        /// <value>
        /// An array of parameterizable strings that may appear in formatted
        /// log messages.
        /// </value>
        public virtual string[] Parameters
        {
            get { return m_parameters; }
            set { m_parameters = value; }
        }

        /// <summary>
        /// Gets or sets the logging destination.
        /// </summary>
        /// <remarks>
        /// Can be one of Error, Out, or a file name.
        /// </remarks>
        /// <value>
        /// The logging destination.
        /// </value>
        public virtual string Destination
        {
            get { return m_destination; }
            set { m_destination = value; }
        }

        /// <summary>
        /// Gets or sets the assembly product.
        /// </summary>
        internal virtual string Product
        {
            get { return m_product; }
            set { m_product = value; }
        }

        /// <summary>
        /// Gets or sets the assembly version.
        /// </summary>
        internal virtual string Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        /// <summary>
        /// Gets or sets the assembly build info.
        /// </summary>
        internal string BuildInfo
        {
           get { return m_buildInfo; }
           set { m_buildInfo = value; }
        }

        /// <summary>
        /// Gets or sets the assembly build type.
        /// </summary>
        internal string BuildType
        {
            get { return m_buildTarget; }
            set { m_buildTarget = value; }
        }

        /// <summary>
        /// Gets or sets the assembly copyright.
        /// </summary>
        internal string Copyright
        {
           get { return m_copyright; }
           set { m_copyright = value; }
        }

        /// <summary>
        /// Gets or sets the product edition.
        /// </summary>
        internal virtual string Edition
        {
            get { return m_edition; }
            set { m_edition = value; }
        }

        #endregion

        #region Constructor and initialization methods

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Logger()
        {
            Init();
        }

        /// <summary>
        /// Initializing state and thread priority.
        /// </summary>
        public void Init()
        {
            DaemonState = 0;
            Priority    = ThreadPriority.BelowNormal;
        }

        /// <summary>
        /// Static initializer.
        /// </summary>
        static Logger()
        {
            string[] a0 = new string[7];
            {
                a0[0] = "{date}";
                a0[1] = "{level}";
                a0[2] = "{thread}";
                a0[3] = "{e}";
                a0[4] = "{text}";
                a0[5] = "{product}";
                a0[6] = "{version}";
            }
            s_defaultParameters = a0;
            string[] a1 = new string[10];
            {
                a1[0] = "Internal";
                a1[1] = "Error";
                a1[2] = "Warning";
                a1[3] = "Info";
                a1[4] = "D4";
                a1[5] = "D5";
                a1[6] = "D6";
                a1[7] = "D7";
                a1[8] = "D8";
                a1[9] = "D9";
            }
            LEVEL_TEXT = a1;

            InitStatic();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Initialization of static array of Log levels.
        /// </summary>
        protected static void InitStatic()
        {
            int[] integers = new int[] { LEVEL_NONE, LEVEL_INTERNAL, LEVEL_ERROR, LEVEL_WARNING, LEVEL_INFO, LEVEL_D4, LEVEL_D5, LEVEL_D6, LEVEL_D7, LEVEL_D8, LEVEL_D9, LEVEL_ALL };

            Integer = integers;
        }

        /// <summary>
        /// Return <b>true</b> if the Logger would log a message with the
        /// given log level.
        /// </summary>
        /// <param name="level">
        /// The log level.
        /// </param>
        public virtual bool IsEnabled(int level)
        {
            return GetInteger(level) <= Level;
        }

        /// <summary>
        /// Configure a newly created Logger instance using the supplied
        /// OperationalContext.
        /// </summary>
        /// <remarks>
        /// destination
        /// -specifies the output device used by the logging system; can be
        /// one of Error, Out, a file name
        /// <p/>
        /// severity-level
        /// -specifies which logged messages are to be displayed
        /// <p/>
        /// message-format
        /// -specifies how messages that have a logging level specified will
        /// be formatted in the Log
        /// <p/>
        /// character-limit
        /// -specifies the maximum number of characters that the logger
        /// daemon will process from the message queue before discarding all
        /// remaining messages in the queue
        /// <p/>
        /// See the coherence.xsd for additional documentation for each of
        /// these parameters.
        /// </remarks>
        public virtual void Configure(IOperationalContext operationalContext)
        {
            string destination = operationalContext.LogDestination;
            string format      = operationalContext.LogMessageFormat;
            int    level       = operationalContext.LogLevel;
            int    limit       = operationalContext.LogCharacterLimit;

            // validate the log level
            if (level < LEVEL_NONE)
            {
                level = LEVEL_NONE;
            }
            else if (level > LEVEL_ALL)
            {
                level = LEVEL_ALL;
            }

            // validate the log level
            if (limit <= 0)
            {
                limit = Int32.MaxValue;
            }

            // create a LogOutput of the appropriate type
            LogOutput output;
            try
            {
                if ("common-logger" == destination.ToLower())
                {
                    output = new CommonLoggingLogger();
                }
                else
                {
                    output = new Standard();
                }
                output.Configure(operationalContext);
            }
            catch (Exception e)
            {
                output = new Standard();
                output.Log(GetInteger(LEVEL_ERROR), e, 
                        "Error configuring logger; using default settings.");
            }

            Destination = destination;
            Format      = format;
            Level       = level;
            Limit       = limit;
            LogOutput   = output;
            Edition     = operationalContext.EditionName;

            Assembly assembly = Assembly.GetExecutingAssembly();

            Copyright = ((AssemblyCopyrightAttribute)assembly.GetCustomAttributes(
                typeof(AssemblyCopyrightAttribute), true)[0]).Copyright;
            BuildType = ((AssemblyConfigurationAttribute)assembly.GetCustomAttributes(
                typeof(AssemblyConfigurationAttribute), true)[0]).Configuration;
            Product   = ((AssemblyProductAttribute)assembly.GetCustomAttributes(
                typeof(AssemblyProductAttribute), true)[0]).Product;

            // N.N.N.N [DPR|RC|Internal|] Build NNNNN
            string description = ((AssemblyDescriptionAttribute)assembly.GetCustomAttributes(
                typeof(AssemblyDescriptionAttribute), true)[0]).Description;
            int i = description.IndexOf("Build");
            if (i == -1)
            {
                BuildInfo = "0";
                Version   = description.Trim();
            }
            else
            {
                BuildInfo = description.Substring(i + 5).Trim();
                Version   = description.Substring(0, i).Trim();
            }

            i = Version.IndexOf(" ");
            if (i > 0)
            {
                Version = StringUtils.ToOracleVersion(assembly.GetName().Version) + Version.Substring(i);
            }
            else
            {
                Version = Version.IndexOf('.') > 0
                    ? StringUtils.ToOracleVersion(assembly.GetName().Version)
                    : StringUtils.ToOracleVersion(assembly.GetName().Version) + " " + Version;
            }

            if (Parameters == null)
            {
                Parameters = DefaultParameters;
            }
        }

        /// <summary>
        /// Create and initialize a new Logger instance using the supplied
        /// XML configuration:
        /// <p/>
        /// destination
        /// -specifies the output device used by the logging system; can be
        /// one of Error, Out or a file name
        /// <p/>
        /// severity-level
        /// -specifies which logged messages are to be displayed
        /// <p/>
        /// message-format
        /// -specifies how messages that have a logging level specified will
        /// be formatted in the Log
        /// <p/>
        /// character-limit
        /// -specifies the maximum number of characters that the logger
        /// daemon will process from the message queue before discarding all
        /// remaining messages in the queue
        /// <p/>
        /// See the coherence.xsd for additional documentation for each of
        /// these parameters.
        /// </summary>
        /// <param name="xmlConfig">
        /// <b>IXmlElement</b> with configuration information.
        /// </param>
        /// <returns>
        /// New Logger instance configured using supplied xml configuration.
        /// </returns>
        public static Logger CreateLogger(IXmlElement xmlConfig)
        {
            return null;
        }

        /// <summary>
        /// Create a new Log message.
        /// </summary>
        /// <remarks>
        /// The Log message has the following structure:
        ///
        /// new Object[]
        /// {
        /// [Timestamp],
        /// [Level],
        /// [Thread],
        /// [Throwable],
        /// [Message],
        /// [Parameter Value]*
        /// };
        /// </remarks>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        /// <param name="message">
        /// Message to log.
        /// </param>
        /// <param name="parameterValues">
        /// An array of parameter values to be used when formatting message.
        /// </param>
        /// <returns>
        /// An array of objects representing log message.
        /// </returns>
        protected virtual object[] CreateMessage(int level, Exception exception, string message, object[] parameterValues)
        {
            int      countParam   = parameterValues == null ? 0 : parameterValues.Length;
            object[] messageArray = new object[5 + countParam];

            messageArray[0] = DateTime.Now; //new DateTime((DateTime.Now.Ticks - 621355968000000000) / 10000 * 10000L + 621355968000000000);
            messageArray[1] = GetInteger(level);
            messageArray[2] = Thread.CurrentThread;
            messageArray[3] = exception;
            messageArray[4] = message;

            if (countParam > 0)
            {
                Array.Copy(parameterValues, 0, messageArray, 5, countParam);
            }

            return messageArray;
        }

        /// <summary>
        /// Format the given Log message by parameterizing the message
        /// format string with the values contained in the given message.
        /// </summary>
        /// <remarks>
        /// Log messages must be in the form of an Object array with the
        /// following structure:
        ///
        /// new Object[]
        /// {
        /// [Timestamp],
        /// [Level],
        /// [Thread],
        /// [Throwable],
        /// [Message],
        /// [Parameter Value]*
        /// };
        /// </remarks>
        /// <param name="message">
        /// Log message to format.
        /// </param>
        /// <returns>
        /// Formatted message text.
        /// </returns>
        public virtual string FormatMessage(object[] message)
        {
            string text = (string) message[4];

            // do not format messages with LEVEL_INTERNAL
            Int32 level = (Int32) message[1];
            if (level != LEVEL_INTERNAL)
            {
                text = Format;

                // replace any parameters
                string[] parameters = Parameters;
                for (int i = 0; i < parameters.Length; i++)
                {
                    string paramName  = parameters[i];
                    object paramValue = i < message.Length ? message[i] : null;
                    text = text.Replace(paramName, FormatParameter(paramName, paramValue));
                }
            }

            return text;
        }

        /// <summary>
        /// Format the given parameter with the given name for output to the
        /// underlying logger.
        /// </summary>
        /// <param name="name">
        /// Parameter name.
        /// </param>
        /// <param name="value">
        /// Parameter value.
        /// </param>
        /// <returns>
        /// Formatted parameter.
        /// </returns>
        protected virtual string FormatParameter(string name, object value)
        {
            string param = null;

            if (name != null && name.Length > 2)
            {
                switch (name[1])
                {
                    case 'd':
                        // {date}
                        if (name.Equals("{date}") && value is DateTime)
                        {
                            param = ((DateTime) value).ToString(TIME_PATTERN);
                        }
                        break;

                    case 'l':
                        // {level}
                        if (name.Equals("{level}") && value is Int32)
                        {
                            param = LEVEL_TEXT[((Int32) value)];
                        }
                        break;

                    case 'p':
                        // {product}
                        if (name.Equals("{product}"))
                        {
                            String product = (Product + " " + Edition).Trim();
                            if (product != null && product.Length > 0)
                            {
                                param = product;
                            }
                        }
                        break;

                    case 't':
                        // {thread}
                        if (name.Equals("{thread}") && value is Thread)
                        {
                            param = ((Thread) value).Name;
                        }
                        // {text}
                        else if (name.Equals("{text}"))
                        {
                            param = value is string ? (string) value : "";
                        }
                        break;

                    case 'v':
                        // {version}
                        if (name.Equals("{version}"))
                        {
                            String version = Version;
                            if (version != null && version.Length > 0)
                            {
                                param = version;
                            }
                        }
                        break;

                    case 'b':
                        // {banner}
                        if (name.Equals("{buildinfo}"))
                        {
                           if (BuildInfo != null && BuildInfo.Length > 0)
                           {
                              param = BuildInfo;
                           }
                        }
                        break;
                }
            }

            if (param == null)
            {
                return value == null ? "n/a" : value.ToString();
            }

            return param;
        }

        /// <summary>
        /// Returns value from a cache of frequently used Integer objects
        /// that represent logging levels.
        /// </summary>
        /// <param name="index">
        /// Logging level.
        /// </param>
        /// <returns>
        /// Logging level value between <see cref="LEVEL_INTERNAL"/> and
        /// <see cref="LEVEL_D9"/>.
        /// </returns>
        /// <seealso cref="Integer"/>
        protected static int GetInteger(int index)
        {
            index = index < LEVEL_INTERNAL ? LEVEL_INTERNAL : index;
            index = index > LEVEL_D9 ? LEVEL_D9 : index;

            return Integer[index + 1];
        }

        /// <summary>
        /// Log the given message with the specified Log level.
        /// </summary>
        /// <remarks>
        /// The supplied object array will be used to format the Log message.
        /// </remarks>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="message">
        /// Message text.
        /// </param>
        /// <param name="parameters">
        /// Parameters used for message formatting.
        /// </param>
        public virtual void Log(int level, string message, object[] parameters)
        {
            Log(level, null, message, parameters);
        }

        /// <summary>
        /// Log the given Exception with the specified Log level.
        /// </summary>
        /// <remarks>
        /// The supplied object array will be used to format the Log message.
        /// </remarks>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        /// <param name="parameters">
        /// Parameters used for message formatting.
        /// </param>
        public virtual void Log(int level, Exception exception, object[] parameters)
        {
            Log(level, exception, null, parameters);
        }

        /// <summary>
        /// Log the given Exception and associated message with the specified
        /// Log level.
        /// </summary>
        /// <remarks>
        /// The supplied object array will be used to format the Log message.
        /// </remarks>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        /// <param name="message">
        /// Message text to log.
        /// </param>
        /// <param name="parameters">
        /// Parameters used to format message.
        /// </param>
        public virtual void Log(int level, Exception exception, string message, object[] parameters)
        {
            level = level < LEVEL_NONE ? LEVEL_NONE : level;
            level = level > LEVEL_ALL ? LEVEL_ALL : level;

            if (level <= Level)
            {
                // create a new Log message and add it to the queue
                object msg = CreateMessage(level, exception, message, parameters);
                Queue.Add(msg);
            }
        }

        /// <summary>
        /// This event occurs when an exception is thrown from OnEnter,
        /// OnWait, OnNotify and onExit.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// If the exception should terminate the daemon, call
        /// <see cref="Tangosol.Util.Daemon.Daemon.Stop"/>. The default
        /// implementation prints debugging information and terminates the
        /// daemon.
        /// </remarks>
        /// <param name="e">
        /// The Exception object.
        /// </param>
        protected override void OnException(Exception e)
        {
            // Log and continue
            Console.Error.WriteLine("Logger: " + e);
        }

        /// <summary>
        /// Called immediately before a Log message is logged to the
        /// underlying LogOutput.
        /// </summary>
        protected virtual void OnLog()
        {}

        /// <summary>
        /// Event notification to perform a regular daemon activity.
        /// </summary>
        /// <remarks>
        /// To get it called, another thread has to set
        /// <see cref="Util.Daemon.Daemon.IsNotification"/> to <b>true</b>:
        /// <code>daemon.IsNotification = true;</code>
        /// </remarks>
        /// <seealso cref="Util.Daemon.Daemon.OnWait"/>
        protected override void OnNotify()
        {
            int  MAX_TOTAL          = Limit;
            int  totalCharCount     = 0;
            bool truncate           = false;
            int  truncateCount      = 0;
            int  truncateLinesCount = 0;
            int  truncateCharCount  = 0;
            bool isDone             = false;

            do
            {
                object[] message = (object[]) Queue.RemoveNoWait();

                // check for end of queue; if any messages have been discarded, report the
                // number and size
                if (message == null)
                {
                    if (truncate && truncateCount > 0)
                    {
                        message = new object[] { DateTime.Now, GetInteger(LEVEL_WARNING), Thread.CurrentThread, null,
                            "Asynchronous logging character limit exceeded; discarding " +
                            truncateCount + " Log messages " + "(lines=" + truncateLinesCount +
                            ", chars=" + truncateCharCount + ")" };
                        isDone = true;
                    }
                    else
                    {
                        break;
                    }
                }

                if (message.Length == 0)
                {
                    // zero length message array serves as a shutdown marker
                    IsExiting = true;
                    return;
                }

                int level = (int) message[1];
                if (level > Level)
                {
                    // Log level must have been changed after start
                    continue;
                }

                string    text      = FormatMessage(message);
                string    textSafe  = text == null ? String.Empty : text;
                Exception exception = message[3] as Exception;
                string    exc       = exception == null ? String.Empty
                                                        : (exception.StackTrace == null ? String.Empty
                                                                                        : exception.StackTrace);

                totalCharCount += textSafe.Length + exc.Length;
                if (truncate && !isDone)
                {
                    truncateCount      += 1;
                    truncateLinesCount += textSafe.Split('\n').Length;
                    truncateLinesCount += exc.Split('\n').Length;
                    truncateCharCount  += textSafe.Length;
                    truncateCharCount  += exc.Length;
                }
                else
                {
                    if (totalCharCount > MAX_TOTAL)
                    {
                        truncate = true;
                    }

                    OnLog();
                    if (exception == null)
                    {
                        LogOutput.Log(level, textSafe);
                    }
                    else if (text == null)
                    {
                        LogOutput.Log(level, exception);
                    }
                    else
                    {
                        LogOutput.Log(level, exception, textSafe);
                    }
                }
            }
            while (!isDone);
        }

        /// <summary>
        /// A cache of frequently used Integer objects that represent logging
        /// levels.
        /// </summary>
        /// <param name="index">
        /// Index of specified <paramref name="integer"/> in the cache.
        /// </param>
        /// <param name="integer">
        /// Value to be set.
        /// </param>
        protected static void SetInteger(int index, ref Int32 integer)
        {
            Integer[index] = integer;
        }

        /// <summary>
        /// Stop the Logger and release any resources held by the Logger.
        /// </summary>
        /// <remarks>
        /// This method has no effect if the Logger has already been stopped.
        /// Stopping a Logger makes it unusable. Any attempt to use a stopped
        /// Logger may result in an exception.
        /// </remarks>
        public virtual void Shutdown()
        {
            lock (this)
            {
                if (IsStarted)
                {
                    // zero length Log info serves as a shutdown marker
                    Queue.Add(new object[0]);
                    try
                    {
                        Monitor.Wait(this, 1000);
                    }
                    catch (Exception)
                    {
                        Stop();
                    }
                    finally
                    {
                        LogOutput.Close();
                    }
                }
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// A delimiter used to decorate service thread names with
        /// information useful for a thread dump analysis.
        /// </summary>
        public static readonly char THREAD_NAME_DELIM = '|';

        /// <summary>
        /// The default set of parameterizable strings that may appear in
        /// formatted Log messages.
        /// </summary>
        private static string[] s_defaultParameters;

        /// <summary>
        /// The logging destination.
        /// </summary>
        private string m_destination;

        /// <summary>
        /// The Log message format template.
        /// </summary>
        private string m_format;

        /// <summary>
        /// A cache of frequently used Integer objects that represent logging
        /// levels.
        /// </summary>
        private static int[] s_integer;

        /// <summary>
        /// The logging level.
        /// </summary>
        private int m_level;

        /// <summary>
        /// Logging level associated with internal Log messages.
        /// </summary>
        public const int LEVEL_INTERNAL = 0;

        /// <summary>
        /// Logging level associated with error messages.
        /// </summary>
        public const int LEVEL_ERROR = 1;

        /// <summary>
        /// Logging level associated with warning messages.
        /// </summary>
        public const int LEVEL_WARNING = 2;

        /// <summary>
        /// Logging level associated with informational messages.
        /// </summary>
        public const int LEVEL_INFO = 3;

        /// <summary>
        /// Logging level associated with debug messages.
        /// </summary>
        public const int LEVEL_D4 = 4;

        /// <summary>
        /// Logging level associated with debug messages.
        /// </summary>
        public const int LEVEL_D5 = 5;

        /// <summary>
        /// Logging level associated with debug messages.
        /// </summary>
        public const int LEVEL_D6 = 6;

        /// <summary>
        /// Logging level associated with debug messages.
        /// </summary>
        public const int LEVEL_D7 = 7;

        /// <summary>
        /// Logging level associated with debug messages.
        /// </summary>
        public const int LEVEL_D8 = 8;

        /// <summary>
        /// Logging level associated with debug messages.
        /// </summary>
        public const int LEVEL_D9 = 9;

        /// <summary>
        /// Logging level that instructs the Logger to log all messages.
        /// </summary>
        public const int LEVEL_ALL = 10;

        /// <summary>
        /// Logging level that instructs the Logger to not log any messages.
        /// </summary>
        public const int LEVEL_NONE = -1;

        /// <summary>
        /// A String array containing descriptions of each of the supported
        /// logging levels indexed by the level.
        /// </summary>
        public static string[] LEVEL_TEXT;

        /// <summary>
        /// Time pattern to use when printing log messages.
        /// </summary>
        private const string TIME_PATTERN = "yyyy'-'MM'-'dd HH':'mm':'ss'.'fff";

        /// <summary>
        /// The logging character limit.
        /// </summary>
        private int m_limit;

        /// <summary>
        /// The LogOutput used to log all formatted Log messages.
        /// </summary>
        private LogOutput m_logOutput;

        /// <summary>
        /// The set of parameterizable strings that may appear in formatted
        /// Log messages.
        /// </summary>
        private string[] m_parameters;

        /// <summary>
        /// The assembly product.
        /// </summary>
        private string m_product;

        /// <summary>
        /// The assembly version.
        /// </summary>
        private string m_version;

        /// <summary>
        /// The assembly build info.
        /// </summary>
        private string m_buildInfo;

        /// <summary>
        /// The assembly build target.
        /// </summary>
        private string m_buildTarget;

        /// <summary>
        /// The assembly copyright.
        /// </summary>
        private string m_copyright;

        /// <summary>
        /// The product edition.
        /// </summary>
        private string m_edition;

        #endregion
    }
}