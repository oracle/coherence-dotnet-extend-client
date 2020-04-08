/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.Net;

namespace Tangosol.Util.Logging
{
    /// <summary>
    /// Concrete <see cref="LogOutput"/> extension that logs messages to
    /// either <b>Error</b>, <b>Out</b>, or a file via a <b>TextWriter</b>.
    /// </summary>
    /// <remarks>
    /// The DefaultLogOutput takes the following configuration parameters:
    ///
    /// destination
    ///     - specifies the output device used by the logging system; can be
    ///       one of Error, Out, or a file name
    ///
    /// See the coherence.xsd for additional documentation for each of these
    /// parameters.
    /// </remarks>
    public class Standard : LogOutput
    {
        #region Properties

        /// <summary>
        /// Gets or sets the <b>TextWriter</b> used to output log messages.
        /// </summary>
        /// <value>
        /// <b>TextWriter</b> used to output log messages.
        /// </value>
        protected virtual TextWriter PrintStream
        {
            get { return m_printStream; }
            set { m_printStream = value; }
        }

        #endregion

        #region LogOutput implementation

        /// <summary>
        /// Close the LogOutput and release any resources held by the
        /// LogOutput.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method has no effect if the LogOutput has already been
        /// closed. Closing a LogOutput makes it unusable. Any attempt to use
        /// a closed LogOutput may result in an exception.
        /// </remarks>
        public override void Close()
        {
            base.Close();

            TextWriter stream = PrintStream;
            stream.Flush();
            if (stream != Console.Out && stream != Console.Error)
            {
                stream.Close();
            }
        }

        /// <summary>
        /// Configure a newly created LogOutput instance using the supplied
        /// OperationalContext.
        /// </summary>
        /// <param name="operationalContext">
        /// Operational context used to configure this LogOutput object.
        /// </param>
        public override void Configure(IOperationalContext operationalContext)
        {
            base.Configure(operationalContext);

            TextWriter stream      = null;
            string     destination = operationalContext.LogDestination;
            string     error       = null;

            if (StringUtils.IsNullOrEmpty(destination) || destination.ToLower().Equals("stderr"))
            {
                stream = Console.Error;
            }
            else if (destination.ToLower().Equals("stdout"))
            {
                stream = Console.Out;
            }
            else
            {
                try
                {
                    if (Directory.Exists(destination))
                    {
                        error = "\nThe specified log file \""
                         + destination
                         + "\" refers to a directory";
                    }
                    else
                    {
                        FileStream fs;
                        FileInfo   file = new FileInfo(destination);
                        if (!file.Exists)
                        {
                            if (!file.Directory.Exists)
                            {
                                file.Directory.Create();
                            }
                            fs = file.Create();
                            fs.Close();
                        }
                        fs     = file.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
                        stream = new StreamWriter(fs);
                    }
                }
                catch (Exception e)
                {
                    error = "\nError opening the specified log file \""
                     + destination
                     + "\" ("
                     + e.Message
                     + ")";
                }

                if (stream == null)
                {
                    if (error != null)
                    {
                        error += "; using Console.Error for log output instead.\n";
                        Console.Error.WriteLine(error);
                    }

                    stream = Console.Error;
                }
            }

            PrintStream = stream;
        }

        /// <summary>
        /// Log the given message with the specified Log level (specific to
        /// the underlying logging mechanism).
        /// </summary>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="message">
        /// Message to log.
        /// </param>
        protected override void Log(object level, string message)
        {
            Log(level, null, message);
        }

        /// <summary>
        /// Log the given Exception with the specified Log level (specific to
        /// the underlying logging mechanism).
        /// </summary>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        protected override void Log(object level, Exception exception)
        {
            Log(level, exception, null);
        }

        /// <summary>
        /// Log the given Exception and associated message with the
        /// specified Log level (specific to the underlying logging
        /// mechanism).
        /// </summary>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        /// <param name="message">
        /// Message to log.
        /// </param>
        protected override void Log(object level, Exception exception, string message)
        {
            TextWriter stream = PrintStream;

            lock (stream)
            {
                if (message != null)
                {
                    stream.WriteLine(message);
                }

                if (exception != null)
                {
                    stream.WriteLine(exception);
                }

                stream.Flush();
            }
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
        protected override object TranslateLevel(Int32 level)
        {
            return level;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The TextWriter used to output log messages.
        /// </summary>
        private TextWriter m_printStream = Console.Error;

        #endregion
    }
}