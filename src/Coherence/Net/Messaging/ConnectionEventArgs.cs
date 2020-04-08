/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Text;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An event which indicates that an <see cref="IConnection"/> was:
    /// <list type="bullet">
    /// <item>opened</item>
    /// <item>closed</item>
    /// <item>determined to be unusable</item>
    /// </list>
    /// </summary>
    /// <author>Jason Howes  2006.03.28</author>
    /// <author>Ivan Cikic  2006.11.08</author>
    /// <since>Coherence 3.2</since>
    /// <seealso cref="ConnectionEventHandler"/>
    public class ConnectionEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Return the <see cref="IConnection"/> associated with this event.
        /// </summary>
        /// <value>
        /// The <see cref="IConnection"/>.
        /// </value>
        public virtual IConnection Connection
        {
            get { return m_connection; }
        }

        /// <summary>
        /// Return connection event type.
        /// </summary>
        /// <value>
        /// The event type, one of <see cref="ConnectionEventType"/>
        /// enumeration values.
        /// </value>
        public virtual ConnectionEventType EventType
        {
            get { return m_eventType; }
        }

        /// <summary>
        /// Return the optional <b>Exception</b> associated with this event.
        /// </summary>
        /// <value>
        /// This method will usually return a <c>null</c> value if the event
        /// identifier is anything but
        /// <see cref="ConnectionEventType.Error"/>.
        /// </value>
        public virtual Exception Exception
        {
            get { return m_exception; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new <b>ConnectionEventArgs</b>.
        /// </summary>
        /// <param name="connection">
        /// The <see cref="IConnection"/> for which the event applies.
        /// </param>
        /// <param name="eventType">
        /// The event's type, one of <see cref="ConnectionEventType"/>
        /// enumeration values.
        /// </param>
        public ConnectionEventArgs(IConnection connection, ConnectionEventType eventType)
                : this(connection, eventType, null)
        {}

        /// <summary>
        /// Construct a new <b>ConnectionEventArgs</b>.
        /// </summary>
        /// <param name="connection">
        /// The <see cref="IConnection"/> for which the event applies.
        /// </param>
        /// <param name="eventType">
        /// The event's type, one of <see cref="ConnectionEventType"/>
        /// enumeration values.
        /// </param>
        /// <param name="exc">
        /// An optional <b>Exception</b> associated with the event.
        /// </param>
        public ConnectionEventArgs(IConnection connection, ConnectionEventType eventType, Exception exc)
        {
            m_connection = connection;

            if (eventType < ConnectionEventType.Opened || eventType > ConnectionEventType.Error)
            {
                throw new ArgumentException();
            }

            m_eventType = eventType;
            m_exception = exc;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a string representation of this ConnectionEventArgs
        /// object.
        /// </summary>
        /// <returns>
        /// A string representation of this object.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + '{' + GetDescription() + '}';
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Get the event's description.
        /// </summary>
        /// <returns>
        /// This event's description.
        /// </returns>
        protected virtual string GetDescription()
        {
            StringBuilder sb = new StringBuilder(DESCRIPTIONS[(int) EventType])
                    .Append(' ')
                    .Append(Connection);

            Exception exception = Exception;
            if (exception != null)
            {
                sb.Append(' ').Append(exception);
            }

            return sb.ToString();
        }

        #endregion

        #region Data members

        /// <summary>
        /// The <b>IConnection</b> for which the event applies.
        /// </summary>
        private IConnection m_connection;

        /// <summary>
        /// The event identifier.
        /// </summary>
        private ConnectionEventType m_eventType;

        /// <summary>
        /// An optional <b>Exception</b> associated with the event.
        /// </summary>
        private Exception m_exception;

        /// <summary>
        /// Descriptions of the various event types.
        /// </summary>
        private static readonly string[] DESCRIPTIONS = {"<unknown>", "OPENED", "CLOSED", "ERROR"};

        #endregion
    }

    #region Enum: ConnectionEventType

    /// <summary>
    /// Connect event type enumeration.
    /// </summary>
    public enum ConnectionEventType
    {
        /// <summary>
        /// This event identifier indicates that an <see cref="IConnection"/>
        /// has been established.
        /// </summary>
        Opened = 1,

        /// <summary>
        /// This event identifier indicates that an <see cref="IConnection"/>
        /// was closed.
        /// </summary>
        Closed = 2,

        /// <summary>
        /// This event identifier indicates that an <see cref="IConnection"/>
        /// has failed.
        /// </summary>
        Error  = 3
    }

    #endregion
}