/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Net;

namespace Tangosol.Util.Logging
{
    /// <summary>
    /// Abstract component used to log formatted messages to an underlying
    /// logging mechanism.
    /// </summary>
    /// <remarks>
    /// Concrete subclasses must implement the three abstract Log methods:
    ///
    /// Log(Object level, String message, Exception exception)
    /// Log(Object level, String message)
    /// Log(Object level, Exception exception)
    ///
    /// Additionally, a concrete LogOutput must be able to translate between
    /// internal Logger Log levels (see the Logger class for details on
    /// the various levels) and equivalent Log level objects appropriate for
    /// the underlying logging mechanism.
    /// See the <see cref="TranslateLevel"/> method for additional details.
    /// </remarks>
    /// <author>Goran Milosavljevic  2006.09.19</author>
    public abstract class LogOutput
    {
        //CLOVER:OFF
        /// <summary>
        /// Configure a newly created LogOutput instance using the supplied
        /// OperationalContext.
        /// </summary>
        /// <param name="operationalContext">
        /// Operational context used to configure this LogOutput object.
        /// </param>
        public virtual void Configure(IOperationalContext operationalContext)
        {}

        /// <summary>
        /// Close the LogOutput and release any resources held
        /// by the LogOutput.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method has no effect if the LogOutput has already been
        /// closed. Closing a LogOutput makes it unusable. Any attempt to
        /// use a closed LogOutput may result in an exception.
        /// </remarks>
        public virtual void Close()
        {}
        //CLOVER:ON

        /// <summary>
        /// Log the given message with the specified Logger Log level (in
        /// Int32 form).
        /// </summary>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="message">
        /// Message to log.
        /// </param>
        public virtual void Log(int level, string message)
        {
            Log(TranslateLevel(level), message);
        }

        /// <summary>
        /// Log the given Exception with the specified Logger Log level (in
        /// Int32 form).
        /// </summary>
        /// <param name="level">
        /// Logging level.
        /// </param>
        /// <param name="exception">
        /// Exception to log.
        /// </param>
        public virtual void Log(int level, Exception exception)
        {
            Log(TranslateLevel(level), exception);
        }

        /// <summary>
        /// Log the given Exception and associated message with the
        /// specified Logger Log level (in Int32 form).
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
        public virtual void Log(int level, Exception exception, string message)
        {
            Log(TranslateLevel(level), exception, message);
        }

        //CLOVER:OFF
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
        protected virtual void Log(object level, string message)
        {}

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
        protected virtual void Log(object level, Exception exception)
        {}

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
        protected virtual void Log(object level, Exception exception, string message)
        {}
        //CLOVER:ON

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
        protected virtual object TranslateLevel(int level)
        {
            return null;
        }
    }
}