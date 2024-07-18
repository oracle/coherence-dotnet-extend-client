/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System.Collections;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// "Safe" wrapper for Remote Invocation service.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.11.08</author>
    public class SafeInvocationService : SafeService, IInvocationService
    {
        #region Properties

        /// <summary>
        /// Calculated property that returns the running wrapped
        /// <b>IInvocationService</b>.
        /// </summary>
        /// <value>
        /// The wrapped <b>IInvocationService</b>.
        /// </value>
        public virtual IInvocationService RunningInvocationService
        {
            get { return EnsureRunningInvocationService(); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SafeInvocationService()
        {
            SafeServiceState = ServiceState.Initial;
        }

        #endregion

        #region IInvocationService implementation

        /// <summary>
        /// Synchronously invoke the specified task on each of the specified
        /// members.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method will not return until the specified members have
        /// completed their processing, failed in their processing, or died
        /// trying.</p>
        /// <p>
        /// <b>IMember</b>s that are specified but are not currently running
        /// the <b>IInvocationService</b> will not invoke the specified
        /// <see cref="IInvocable"/> object.</p>
        /// <p>
        /// <b>IMember</b>s that leave (gracefully or otherwise) before the
        /// invocation completes will not register a result, and the amount
        /// of processing that completed is indeterminate. <b>IMember</b>s
        /// that encounter an exception during invocation will not be
        /// retried; whatever result has been registered by that point by
        /// that member for that <b>IInvocable</b> object will be returned.
        /// Specifically, the result for a given member will be null under
        /// the following conditions:</p>
        /// <list type="bullet">
        /// <item>if the member did not exist</item>
        /// <item>if the member was not running the service at the time that
        /// the query method was invoked</item>
        /// <item>if the member left (via the shutdown or stop methods, or
        /// unexpectedly) before responding</item>
        /// <item>if the member encountered an exception while processing
        /// and had not registered a non-null result</item>
        /// <item>if the member completed successfully but registered no
        /// result</item>
        /// <item>if the member completed successfully but explicitly
        /// registered a result of null</item>
        /// </list>
        /// </remarks>
        /// <param name="task">
        /// The <b>IInvocable</b> object to distribute to the specified
        /// members in order to be invoked on those members.
        /// </param>
        /// <param name="col">
        /// Optional collection of cluster members to which the
        /// <b>IInvocable</b> object will be distributed; if <c>null</c>, the
        /// <b>IInvocable</b> object will be distributed to all cluster
        /// members that are running this service.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> of result objects keyed by <see cref="IMember"/>
        /// object.
        /// </returns>
        public IDictionary Query(IInvocable task, ICollection col)
        {
            return RunningInvocationService.Query(task, col);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Return the wrapped <b>IInvocationService</b>.
        /// </summary>
        /// <remarks>
        /// This method ensures that the returned <b>IInvocationService</b> is
        /// running before returning it. If the <b>IInvocationService</b> is
        /// not running and has not been explicitly stopped, the
        /// <b>IInvocationService</b> is restarted.
        /// </remarks>
        /// <returns>
        /// The running wrapped <b>IInvocationService</b>.
        /// </returns>
        public virtual IInvocationService EnsureRunningInvocationService()
        {
            return (IInvocationService) EnsureRunningService();
        }

        #endregion
    }
}
