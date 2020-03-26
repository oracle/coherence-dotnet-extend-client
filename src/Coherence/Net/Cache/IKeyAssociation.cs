/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Util.Filter;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// A KeyAssociation represents a key object that has a natural
    /// association with another key object.
    /// </summary>
    /// <remarks>
    /// The key object and the associated key may refer to entries in the
    /// same or different caches.
    /// <p/>
    /// For example, the information provided by a key that implements
    /// <b>IKeyAssociation</b> may be used to place the key into the same
    /// partition as its associated key.
    /// <p/>
    /// See <see cref="KeyAssociatedFilter"/> for an example of a distributed
    /// query that takes advantage of a custom <b>IKeyAssociation</b>
    /// implementation to dramatically optimize its performance.
    /// </remarks>
    /// <since>Coherence 3.0</since>
    public interface IKeyAssociation
    {
        /// <summary>
        /// Determine the key object to which this key object is associated.
        /// </summary>
        /// <remarks>
        /// The key object returned by this method is often referred to as a
        ///  <i>host key</i>.
        /// </remarks>
        /// <value>
        /// The host key that for this key object, or <c>null</c> if this key
        /// has no association.
        /// </value>
        object AssociatedKey
        {
            get;
        }
    }
}