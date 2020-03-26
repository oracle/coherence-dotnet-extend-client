/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Text;

namespace Tangosol.Util
{
    /// <summary>
    /// An event which indicates that a <see cref="IService"/> state has
    /// changed:
    /// <list type="bullet">
    /// <item>a service is starting</item>
    /// <item>a service has started</item>
    /// <item>a service is stopping</item>
    /// <item>a service has stopped</item>
    /// </list>
    /// </summary>
    /// <author>Jason Howes  2007.11.12</author>
    /// <author>Ana Cikic  2007.12.11</author>
    /// <seealso cref="IService"/>
    public class ServiceEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Return the <see cref="IService"/> that fired the event.
        /// </summary>
        /// <value>
        /// A service that fired the event.
        /// </value>
        public virtual IService Service
        {
            get { return m_source; }
        }

        /// <summary>
        /// Return this event's type.
        /// </summary>
        /// <remarks>
        /// The event type is one of the <see cref="ServiceEventType"/>
        /// enumerated constants.
        /// </remarks>
        /// <value>
        /// An event type.
        /// </value>
        public virtual ServiceEventType EventType
        {
            get { return m_eventType; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new ServiceEventArgs.
        /// </summary>
        /// <param name="service">
        /// The <see cref="IService"/> that fired the event.
        /// </param>
        /// <param name="eventType">
        /// This event's type, one of the <see cref="ServiceEventType"/> enum
        /// values.
        /// </param>
        public ServiceEventArgs(IService service, ServiceEventType eventType)
        {
            m_source    = service;
            m_eventType = eventType;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns a string representation of this ServiceEventArgs object.
        /// </summary>
        /// <returns>
        /// A string representation of this ServiceEventArgs object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("ServiceEventArgs{");
            sb.Append(DESCRIPTIONS[(int) EventType])
                .Append(' ')
                .Append(Service.GetType().Name)
                .Append('}');

            return sb.ToString();
        }

        #endregion

        #region Data Members

        /// <summary>
        /// IService object that fired the event.
        /// </summary>
        private IService m_source;

        /// <summary>
        /// This event's type.
        /// </summary>
        private ServiceEventType m_eventType; 

        /// <summary>
        /// Descriptions of the various event types.
        /// </summary>
        private static readonly string[] DESCRIPTIONS = {"<unknown>", "STARTING", "STARTED", "STOPPING", "STOPPED"};

        #endregion
    }
    
    #region Enum: ServiceEventType

    /// <summary>
    /// Service event type enumeration.
    /// </summary>
    public enum ServiceEventType
    {
        /// <summary>
        /// This event indicates that a service is starting.
        /// </summary>
        Starting = 1,

        /// <summary>
        /// This event indicates that a service has started.
        /// </summary>
        Started = 2,

        /// <summary>
        /// This event indicates that a service is stopping.
        /// </summary>
        Stopping = 3,

        /// <summary>
        /// This event indicates that a service has stopped.
        /// </summary>
        Stopped = 4
    }

    #endregion
}