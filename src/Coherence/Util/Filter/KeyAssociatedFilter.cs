/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Filter
{
    /// <summary>
    /// <see cref="IFilter"/> which limits the scope of another filter
    /// according to the key association information.
    /// </summary>
    /// <remarks>
    /// <p/>
    /// This filter is intended to be used to optimize queries for
    /// partitioned caches that utilize any of the key association
    /// algorithms (by implementing either <b>KeyAssociator</b> or
    /// <b>KeyAssociation</b>) to ensure placement of all associated
    /// entries in the same distributed cache partition (and therefore
    /// in the same storage-enabled cluster node). Using the
    /// <b>KeyAssociatedFilter</b> will instruct the distributed cache
    /// to apply the wrapped filter only to the entries stored at the
    /// cache service node that owns the specified host key.
    /// <p/>
    /// <b>Note 1:</b> This filter must be the outermost filter and cannot
    /// be used as a part of any composite filter
    /// (<see cref="AndFilter"/>, <see cref="OrFilter"/>, etc.)
    /// <p/>
    /// <b>Note 2:</b> This filter is intended to be processed only on the
    /// client side of the partitioned cache service.
    /// <p/>
    /// For example, consider two classes called <i>Parent</i> and
    /// <i>Child</i> that are stored in separate caches using
    /// <i>ParentKey</i> and <i>ChildKey</i> objects respectively.
    /// The Parent and Child classes have a <i>Id</i> property that returns
    /// a Long value that uniquely identifies the object. Similarly, the
    /// ParentKey and ChildKey classes have a <i>Id</i> property that
    /// uniquely identifies the corresponding cached object. Futhermore, the
    /// Child and ChildKey classes include a <i>ParentId</i> property that
    /// returns the Long identifier of the Parent object.
    /// <p/>
    /// There are two ways to ensure that Child objects are collocated with
    /// their Parent objects (in the same storage-enabled cluster node).
    /// <list type="number">
    /// <item>
    /// <description>
    /// Make the ChildKey class implement <b>KeyAssociation</b> as follows:
    /// <code>
    /// public Object AssociatedKey
    /// {
    ///     get { return ParentId; }
    /// }
    /// </code>
    /// and the ParentKey class implement <b>KeyAssociation</b> as follows:
    /// <code>
    /// public Object AssociatedKey
    /// {
    ///     get { return Id; }
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Implement a custom <b>KeyAssociator</b> as follows:
    /// <code>
    /// public object GetAssociatedKey(object key)
    /// {
    ///     if (key is ChildKey)
    ///     {
    ///         return ((ChildKey) key).ParentId;
    ///     }
    ///     else if (key is ParentKey)
    ///     {
    ///         return ((ParentKey) key).Id;
    ///     }
    ///     else
    ///     {
    ///     return null;
    ///     }
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// The first approach requires a trivial change to the ChildKey and
    /// ParentKey classes, whereas the second requires a new class and a
    /// configuration change, but no changes to existing classes.
    /// <p/>
    /// Now, to retrieve all the Child objects of a given Parent using an
    /// optimized query you would do the following:
    /// <code>
    /// ParentKey parentKey = new ParentKey(...);
    /// Long      parentId  = parentKey.Id;
    ///
    /// // this Filter will be applied to all Child objects in order to fetch
    /// // those for which ParentId returns the specified Parent identifier
    /// IFilter filterEq = new EqualsFilter("ParentId", parentId);
    ///
    /// // this Filter will direct the query to the cluster node that
    /// // currently owns the Parent object with the given identifier
    /// IFilter filterAsc = new KeyAssociatedFilter(filterEq, parentId);
    ///
    /// // run the optimized query to get the ChildKey objects
    /// ICollection colChildKeys = cacheChildren.Keys(filterAsc);
    ///
    /// // get all the Child objects at once
    /// ICollection colChildren = cacheChildren.GetAll(colChildKeys);
    /// </code>
    /// To remove the Child objects you would then do the following:
    /// <code>cacheChildren.Keys.RemoveAll(colChildKeys);</code>
    /// </remarks>
    /// <author>Gene Gleyzer  2005.06.09</author>
    /// <author>Jason Howes  2005.11.02</author>
    /// <author>Goran Milosavljevic  2006.10.24</author>
    public class KeyAssociatedFilter : IFilter, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the host key that serves as an associated key for all keys
        /// that the wrapped filter will be applied to.
        /// </summary>
        /// <value>
        /// The host key.
        /// </value>
        public virtual object HostKey
        {
            get { return m_hostKey; }
        }

        /// <summary>
        /// Obtain the wrapped <see cref="IFilter"/>.
        /// </summary>
        /// <value>
        /// The wrapped filter object.
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public KeyAssociatedFilter()
        {}

        /// <summary>
        /// Construct a key associated filter.
        /// </summary>
        /// <param name="filter">
        /// The underlying (wrapped) filter.
        /// </param>
        /// <param name="hostKey">
        /// The host key that serves as an associated key for all keys that
        /// the wrapped filter will be applied to.
        /// </param>
        public KeyAssociatedFilter(IFilter filter, object hostKey)
        {
            if (filter == null || filter is KeyAssociatedFilter)
            {
                throw new ArgumentException("Invalid filter: " + filter);
            }

            if (hostKey == null)
            {
                throw new ArgumentNullException("hostKey");
            }

            m_filter  = filter;
            m_hostKey = hostKey;
        }

        #endregion

        #region IFilter implementation

        /// <summary>
        /// Apply the test to the object.
        /// </summary>
        /// <param name="o">
        /// An object to which the test is applied.
        /// </param>
        /// <returns>
        /// <b>true</b> if the test passes, <b>false</b> otherwise.
        /// </returns>
        public virtual bool Evaluate(object o)
        {
            return m_filter.Evaluate(o);
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {
            m_filter  = (IFilter) reader.ReadObject(0);
            m_hostKey = reader.ReadObject(1);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteObject(0, m_filter);
            writer.WriteObject(1, m_hostKey);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>KeyAssociatedFilter</b> with another object to
        /// determine equality.
        /// </summary>
        /// <remarks>
        /// Two <b>KeyAssociatedFilter</b> objects are considered equal if
        /// the wrapped filters and host keys are equal.
        /// </remarks>
        /// <param name="obj">
        /// The <b>KeyAssociatedFilter</b> to compare to.
        /// </param>
        /// <returns>
        /// <b>true</b> if this <b>KeyAssociatedFilter</b> and the passed
        /// object are equivalent <b>KeyAssociatedFilter</b> objects.
        /// </returns>
        public  override bool Equals(object obj)
        {
            if (obj is KeyAssociatedFilter)
            {
                var that = (KeyAssociatedFilter) obj;
                return Equals(m_filter, that.m_filter) && Equals(m_hostKey, that.m_hostKey);
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the <b>KeyAssociatedFilter</b> object
        /// according to the general <b>object.GetHashCode()</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>KeyAssociatedFilter</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_filter.GetHashCode() + m_hostKey.GetHashCode();
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <b>KeyAssociatedFilter</b>.
        /// </summary>
        /// <returns>
        /// A string description of the <b>KeyAssociatedFilter</b>.
        /// </returns>
        public override string ToString()
        {
            return GetType().Name + '(' + m_filter + ", " + m_hostKey + ')';
        }

        #endregion

        #region Data members

        /// <summary>
        /// The underlying filter.
        /// </summary>
        private IFilter m_filter;

        /// <summary>
        /// The association host key.
        /// </summary>
        private object m_hostKey;

        #endregion
    }
}