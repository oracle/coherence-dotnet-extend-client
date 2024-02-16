/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.IO;

namespace Tangosol.Net
{
    /// <summary>
    /// Represents the method that will handle member event.
    /// </summary>
    /// <remarks>
    /// <p>
    /// MemberEventHandler will be invoked in the order in which they are registered.</p>
    /// </remarks>
    /// <param name="sender">
    /// <see cref="IService"/> that raised an event.
    /// </param>
    /// <param name="args">
    /// Event arguments.
    /// </param>
    public delegate void MemberEventHandler(object sender, MemberEventArgs args);

    /// <summary>
    /// This <see cref="IService"/> interface represents a controllable
    /// service that operates in a clustered network environment.
    /// </summary>
    /// <author>Gene Gleyzer  2002.02.08</author>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    public interface IService : Util.IService
    {
        /// <summary>
        /// Gets the <see cref="IServiceInfo"/> object for this
        /// <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The <b>IServiceInfo</b> object.
        /// </value>
        IServiceInfo Info
        {
            get;
        }

        /// <summary>
        /// Gets or sets the user context object associated with this
        /// <see cref="IService"/>.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// The data type and semantics of this context object are entirely
        /// application specific and are opaque to the <b>IService</b>
        /// itself.
        /// </remarks>
        /// <value>
        /// User context object associated with this <b>IService</b>.
        /// </value>
        /// <since>Coherence 3.0</since>
        object UserContext
        {
            get;
            set;
        }

        /// <summary>
        /// The <see cref="ISerializer"/> used to serialize and deserialize
        /// objects by this <b>IService</b>.
        /// </summary>
        /// <value>
        /// The <b>ISerializer</b> for this <b>IService</b>.
        /// </value>
        ISerializer Serializer { get; }

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has joined the service.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Note: this event could be called during the service restart on
        /// in which case the event handler should not attempt to use any
        /// cache or service functionality.</p>
        /// <p>
        /// The most critical situation arises when a number of threads are
        /// waiting for a local service restart, being blocked by a
        /// <b>IService</b> object synchronization monitor. Since the Joined
        /// event should be fired only once, it is called on a client thread
        /// <b>while holding a synchronization monitor</b>. An attempt to use
        /// other service functionality during this local event notification
        /// may result in a deadlock.</p>
        /// </remarks>
        event MemberEventHandler MemberJoined;

        /// <summary>
        /// Invoked when an <see cref="IMember"/> is leaving the service.
        /// </summary>
        event MemberEventHandler MemberLeaving;

        /// <summary>
        /// Invoked when an <see cref="IMember"/> has left the service.
        /// </summary>
        /// <remarks>
        /// Note: this event could be called during the service restart in
        /// which case the event handler should not attempt to use any cache
        /// or service functionality.
        /// </remarks>
        event MemberEventHandler MemberLeft;
    }
}