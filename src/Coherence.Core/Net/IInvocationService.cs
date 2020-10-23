/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;

namespace Tangosol.Net
{
    /// <summary>
    /// The <b>IInvocationService</b> is an <see cref="IService"/> for
    /// delivering executable objects to a remote cluster member for
    /// invocation.
    /// </summary>
    /// <remarks>
    /// The executable objects must implement the <see cref="IInvocable"/>
    /// interface. Using this interface, application code can execute an 
    /// <b>IInvocable</b> object within the context of a remote cluster.
    /// </remarks>
    /// <author>Cameron Purdy  2003.01.05</author>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    /// <since>Coherence 2.1</since>
    public interface IInvocationService : IService
    {
        /// <summary>
        /// Synchronously invoke the specified task on the cluster member to
        /// which the client is connected.
        /// </summary>
        /// <remarks>
        /// This method will not return until the cluster member has completed
        /// its processing, failed in its processing, or died trying.
        /// </remarks>
        /// <param name="task">
        /// The <b>IInvocable</b> object to send to the cluster member in order
        /// to be invoked on that member.
        /// </param>
        /// <param name="col">
        /// Parameter reserved for future use. Must be null.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> of the result of the invocation, keyed by
        /// the "local" <see cref="IMember"/> object.
        /// </returns>
        IDictionary Query(IInvocable task, ICollection col);
    }
}