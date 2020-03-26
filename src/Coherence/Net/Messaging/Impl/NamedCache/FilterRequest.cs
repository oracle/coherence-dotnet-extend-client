/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// Base class for all NamedCache Protocol
    /// <see cref="NamedCacheRequest"/> messages that include a
    /// <see cref="IFilter"/>.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="NamedCacheRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public abstract class FilterRequest : NamedCacheRequest
    {
        #region Properties

        /// <summary>
        /// The <see cref="IFilter"/> associated with this FilterRequest.
        /// </summary>
        /// <value>
        /// The <b>IFilter</b> associated with this FilterRequest.
        /// </value>
        public virtual IFilter Filter
        {
            get { return m_filter; }
            set { m_filter = value; }
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
        /// <seealso cref="Request.ReadExternal"/>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            Filter = (IFilter) reader.ReadObject(1);
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

            writer.WriteObject(1, Filter);
        }

        #endregion
        
        #region Extend Overrides

        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", Filter=" + Filter;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The IFilter associated with this FilterRequest.
        /// </summary>
        private IFilter m_filter;

        #endregion
    }
}