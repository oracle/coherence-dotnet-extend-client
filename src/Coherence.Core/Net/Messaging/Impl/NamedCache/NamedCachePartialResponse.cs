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
    /// Generic <see cref="IResponse"/> used for partial
    /// <see cref="NamedCacheProtocol"/> responses.
    /// </summary>
    public class NamedCachePartialResponse : PartialResponse
    {
        #region Properties

        /// <summary>
        /// Return the identifier for <b>Message</b> object's class.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        /// <summary>
        /// Return the filter used with the request. This is only used for QueryRequests
        /// that carry a LimitFilter.
        /// </summary>
        /// <value>
        /// A filter returned with the response.
        /// </value>
        public IFilter Filter
        {
            get { return m_filter; }
            set { m_filter = value; }
        }

        /// <summary>
        /// Return the cookie used by the returned filter (See <b>Filter</b> property).
        /// </summary>
        /// <value>
        /// A filter cookie returned with the response.
        /// </value>
        public object FilterCookie
        {
            get { return m_filterCookie; }
            set { m_filterCookie = value; }
        }

        #endregion

        #region IRunnable implementation

        /// <summary>
        /// Execute the action specific to the <see cref="Message"/>
        /// implementation.
        /// </summary>
        public override void Run()
        {}

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
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            // COH-6337
            if (ImplVersion > 2)
            {
                Filter       = (IFilter) reader.ReadObject(7);
                FilterCookie = reader.ReadObject(8);
            }
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
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            // COH-6337
            if (ImplVersion > 2)
            {
                writer.WriteObject(7, Filter);
                writer.WriteObject(8, FilterCookie);
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 1000;

        /// <summary>
        /// The filter to be returned. This is only used for QueryRequests
        /// that carry a LimitFilter.
        /// </summary>
        public IFilter m_filter;

        /// <summary>
        /// The cookie used by the returned LimitFilter (see <b>Filter</b> proporty).
        /// </summary>
        public object m_filterCookie;

        #endregion
    }
}
