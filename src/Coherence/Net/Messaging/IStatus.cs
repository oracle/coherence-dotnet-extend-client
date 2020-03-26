/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// An IStatus represents an asynchronous <see cref="IRequest"/>
    /// sent to a peer.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The status of the <b>IRequest</b> can be determined by checking the
    /// value of the <see cref="IsClosed"/> property. If it returns
    /// <b>false</b>, the request is still in progress. A return value of
    /// <b>true</b> indicates that the request has either completed
    /// successfully, completed unsuccessfully, or has been canceled.</p>
    /// <p>
    /// When the <b>IRequest</b> completes, the <see cref="IResponse"/> sent
    /// by the peer can be retrieved from the <see cref="Response"/>
    /// property. If this property value is <c>null</c>, the <b>IRequest</b>
    /// was explicitly canceled (<see cref="Cancel()"/>).</p>
    /// <p/>
    /// Rather than constantly polling the <b>IRequest</b> for the outcome of
    /// the <b>IRequest</b>, a thread can alternatively use the IStatus to
    /// <see cref="WaitForResponse(long)"/> wait for the <b>IRequest</b> to
    /// complete.
    /// </remarks>
    /// <author>Jason Howes  2006.03.23</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IResponse"/>
    /// <seealso cref="IRequest"/>
    /// <since>Coherence 3.2</since>
    public interface IStatus
    {
        /// <summary>
        /// Determine if the <b>IRequest</b> represented by this IStatus has
        /// been completed successfully, completed unsuccessfully, or
        /// canceled.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <b>IRequest</b> has been completed
        /// successfully, completed unsuccessfully, or canceled; <b>false</b>
        /// if the <b>IRequest</b> is still pending.
        /// </value>
        bool IsClosed { get; }

        /// <summary>
        /// Cancel the <b>IRequest</b> represented by this IStatus.
        /// </summary>
        /// <remarks>
        /// The requestor can call this method when it is no longer
        /// interested in an <b>IResponse</b> or outcome of the
        /// <b>IRequest</b>.
        /// </remarks>
        void Cancel();

        /// <summary>
        /// Cancel the <b>IRequest</b> represented by this <b>IStatus</b> due
        /// to an error condition.
        /// </summary>
        /// <remarks>
        /// After this method is called, the <see cref="Response"/> will
        /// throw this exception (wrapping it if necessary).
        /// </remarks>
        /// <param name="e">
        /// The reason that the <b>IRequest</b> is being cancelled.
        /// </param>
        void Cancel(Exception e);

        /// <summary>
        /// Return the <b>IRequest</b> represented by this IStatus.
        /// </summary>
        /// <value>
        /// The <b>IRequest</b> represented by this IStatus.
        /// </value>
        IRequest Request { get; }

        /// <summary>
        /// Return the <b>IResponse</b> sent by the peer.
        /// </summary>
        /// <remarks>
        /// This property has a non-null value if <see cref="IsClosed"/> is
        /// <b>true</b>.
        /// </remarks>
        /// <value>
        /// The <b>IResponse</b> sent by the peer.
        /// </value>
        /// <exception cref="Exception">
        /// If the <b>IRequest</b> is cancelled.
        /// </exception>
        IResponse Response { get; }

        /// <summary>
        /// Block the calling thread until the <b>IRequest</b> is completed
        /// successfully, completed unsuccessfully, canceled, or a timeout
        /// occurs.
        /// </summary>
        /// <returns>
        /// The <b>IResponse</b>.
        /// </returns>
        /// <exception cref="Exception">
        /// If the <b>IRequest</b> is cancelled, a timeout occurs, or the
        /// waiting thread is interrupted.
        /// </exception>
        IResponse WaitForResponse();

        /// <summary>
        /// Block the calling thread until the <b>IRequest</b> is completed
        /// successfully, completed unsuccessfully, canceled, or a timeout
        /// occurs.
        /// </summary>
        /// <param name="milisec">
        /// The number of milliseconds to wait for the result of the
        /// <b>IRequest</b>; pass zero to block the calling thread
        /// indefinitely.
        /// </param>
        /// <returns>
        /// The <b>IResponse</b>.
        /// </returns>
        /// <exception cref="Exception">
        /// If the <b>IRequest</b> is cancelled, a timeout occurs, or the
        /// waiting thread is interrupted.
        /// </exception>
        IResponse WaitForResponse(long milisec);
    }
}