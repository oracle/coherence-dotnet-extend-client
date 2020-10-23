/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

using Tangosol.Run.Xml;

namespace Tangosol.Util
{
    /// <summary>
    /// IControllable interface represents an object quite oftenly referred
    /// to as a <i>service</i> that usually operates on its own thread and
    /// has a controllable life cycle.
    /// </summary>
    /// <author>Gene Gleyzer  2002.02.08, 2003.02.11</author>
    /// <author>Goran Milosavljevic  2006.08.16</author>
    public interface IControllable
    {
        /// <summary>
        /// Configure the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method can only be called before the controllable service
        /// is started.
        /// </remarks>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/> carrying configuration information
        /// specific to the IControllable object.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service is already running.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the configuration information is invalid.
        /// </exception>
        void Configure(IXmlElement xml);

        /// <summary>
        /// Start the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method should only be called once per the life cycle
        /// of the IControllable service. This method has no affect if the
        /// service is already running.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a service does not support being re-started, and the
        /// service was already started and subsequently stopped and then
        /// an attempt is made to start the service again; also thrown if
        /// the IControllable service has not been configured.
        /// </exception>
        void Start();

        /// <summary>
        /// Determine whether or not the controllable service is running.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// Returns <b>false</b> before a service is started, while the
        /// service is starting, while a service is shutting down and after
        /// the service has stopped. It only returns <b>true</b> after
        /// completing its start processing and before beginning its shutdown
        /// processing.
        /// </remarks>
        /// <returns>
        /// <b>true</b> if the service is running; <b>false</b> otherwise.
        /// </returns>
        bool IsRunning { get; }

        /// <summary>
        /// Stop the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This is a controlled shut-down, and is preferred to the
        /// <see cref="Stop"/> method.
        /// <p/>
        /// This method should only be called once per the life cycle
        /// of the controllable service. Calling this method for a service
        /// that has already stopped has no effect.
        /// </remarks>
        void Shutdown();

        /// <summary>
        /// Hard-stop the controllable service.
        /// </summary>
        /// <remarks>
        /// Use <see cref="Shutdown"/> for normal service termination.
        /// Calling this method for a service that has already stopped has no
        /// effect.
        /// </remarks>
        void Stop();
    }
}