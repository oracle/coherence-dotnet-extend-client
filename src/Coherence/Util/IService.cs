/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util
{
    /// <summary>
    /// Represents the method that will handle service event.
    /// </summary>
    /// <param name="sender">
    /// <see cref="IService"/> that raised an event.
    /// </param>
    /// <param name="args">
    /// Event arguments.
    /// </param>
    public delegate void ServiceEventHandler(object sender, ServiceEventArgs args);

    /// <summary>
    /// A IService is a <see cref="IControllable"/> that emits service
    /// lifecycle events.
    /// </summary>
    /// <author>Jason Howes  2007.11.12</author>
    /// <author>Ana Cikic  2007.12.11</author>
    public interface IService : IControllable
    {
        /// <summary>
        /// Invoked when <see cref="IService"/> is starting.
        /// </summary>
        event ServiceEventHandler ServiceStarting;

        /// <summary>
        /// Invoked when <see cref="IService"/> has started.
        /// </summary>
        event ServiceEventHandler ServiceStarted;

        /// <summary>
        /// Invoked when <see cref="IService"/> is stopping.
        /// </summary>
        event ServiceEventHandler ServiceStopping;

        /// <summary>
        /// Invoked when <see cref="IService"/> has stopped.
        /// </summary>
        event ServiceEventHandler ServiceStopped;
    }
}