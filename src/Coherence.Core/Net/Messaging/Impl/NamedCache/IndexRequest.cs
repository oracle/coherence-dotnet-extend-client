/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The IndexRequest is a <see cref="NamedCacheRequest"/> sent to add or
    /// remove an index on a remote NamedCache.
    /// </summary>
    /// <author>Ivan Cikic  2006.08.30</author>
    /// <seealso cref="NamedCacheRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class IndexRequest : NamedCacheRequest
    {
        #region Properties

        /// <summary>
        /// Return the identifier for <b>Message</b> object's class.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        /// <seealso cref="Message.TypeId"/>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        /// <summary>
        /// The <b>IComparer</b> object which imposes an ordering on entries
        /// in the indexed map; or <c>null</c> if the entries' values natural
        /// ordering should be used.
        /// </summary>
        /// <value>
        /// The <b>IComparer</b> object.
        /// </value>
        public virtual IComparer Comparer
        {
            get { return m_comparer; }
            set { m_comparer = value; }
        }

        /// <summary>
        /// If <b>true</b>, add an index; otherwise remove it.
        /// </summary>
        /// <value>
        /// <b>true</b> to add an index, <b>false</b> to remove it.
        /// </value>
        public virtual bool Add
        {
            get { return m_add; }
            set { m_add = value; }
        }

        /// <summary>
        /// The <see cref="IValueExtractor"/> object that is used to extract
        /// an indexable object from a value stored in the indexed map.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b> object.
        /// </value>
        public virtual IValueExtractor Extractor
        {
            get { return m_extractor; }
            set { m_extractor = value; }
        }

        /// <summary>
        /// If <b>true</b>, the contents of the indexed information should be
        /// ordered.
        /// </summary>
        /// <value>
        /// <b>true</b> if the contents of the indexed information should be
        /// ordered, <b>false</b> otherwise.
        /// </value>
        public virtual bool IsOrdered
        {
            get { return m_isOrdered; }
            set { m_isOrdered = value; }
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.t.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">">
        /// If an I/O error occurs.
        /// </exception>
        /// <seealso cref="Request.ReadExternal"/>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            Add       = reader.ReadBoolean(1);
            Extractor = (IValueExtractor) reader.ReadObject(2);
            IsOrdered = reader.ReadBoolean(3);
            Comparer  = (IComparer) reader.ReadObject(4);
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
        /// <seealso cref="Request.WriteExternal"/>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteBoolean(1, Add);
            writer.WriteObject(2, Extractor);
            writer.WriteBoolean(3, IsOrdered);
            writer.WriteObject(4, Comparer);
        }

        #endregion
         
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription()
                   + ", Add="        + Add
                   + ", Comparator=" + Comparer
                   + ", Extractor="  + Extractor
                   + ", Ordered="    + IsOrdered;
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// If true, add an index; otherwise remove it.
        /// </summary>
        private bool m_add;

        /// <summary>
        /// The Comparer object which imposes an ordering on entries in
        /// the indexed map; or null if the entries' values natural ordering
        /// should be used.
        /// </summary>
        private IComparer m_comparer;

        /// <summary>
        /// The ValueExtractor object that is used to extract an indexable
        /// object from a value stored in the indexed map.
        /// </summary>
        private IValueExtractor m_extractor;

        /// <summary>
        /// If true, the contents of the indexed information should be
        /// ordered.
        /// </summary>
        private bool m_isOrdered;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 42;

        #endregion
    }
}