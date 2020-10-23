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
    /// The QueryRequest is a <see cref="FilterRequest"/> sent to retrieve a
    /// set of keys or entries in a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="FilterRequest"/>
    /// <seealso cref="IFilter"/>
    public class QueryRequest : FilterRequest
    {
        #region Properties

        /// <summary>
        /// Opaque cookie used to support streaming.
        /// </summary>
        /// <seealso cref="PartialResponse.Cookie"/>
        public virtual Binary Cookie
        {
            get { return m_cookie; }
            set { m_cookie = value; }
        }

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
        /// Specifies what kind of response is required: if <b>true</b>,
        /// a keys collection is sent back; otherwise an entries collection.
        /// </summary>
        public virtual bool KeysOnly
        {
            get { return m_keysOnly; }
            set { m_keysOnly = value; }
        }

        /// <summary>
        /// The cookie used by a request that uses a LimitFilter (See <b>Filter</b> property).
        /// </summary>
        /// <value>
        /// A filter cookie for the request.
        /// </value>
        public object FilterCookie
        {
            get { return m_filterCookie; }
            set { m_filterCookie = value; }
        }

        #endregion

        #region Request overrides

        /// <summary>
        /// Create a new <see cref="Response"/> for this IRequest.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="IMessageFactory"/> that must be used to create the
        /// returned <b>Response</b>; never <c>null</c>.
        /// </param>
        /// <returns>
        /// A new <b>Response</b>.
        /// </returns>
        protected override Response InstantiateResponse(IMessageFactory factory)
        {
            return (Response) factory.CreateMessage(NamedCachePartialResponse.TYPE_ID);
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

            KeysOnly     = reader.ReadBoolean(2);
            Cookie       = reader.ReadBinary(3);
            FilterCookie = reader.ReadObject(4);
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

            writer.WriteObject(2, KeysOnly);
            writer.WriteBinary(3, Cookie);
            writer.WriteObject(4, FilterCookie);
        }

        #endregion
        
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() 
                   + ", KeysOnly=" + KeysOnly
                   + ", Cookie="   + Cookie;
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// Specifies what kind of response is required: if <b>true</b>,
        /// a keys collection is sent back; otherwise an entries collection.
        /// </summary>
        private bool m_keysOnly;

        /// <summary>
        /// Opaque cookie used to support streaming.
        /// </summary>
        private Binary m_cookie;

        /// <summary>
        /// The cookie used by a LimitFilter (see <b>Filter</b> proporty).
        /// </summary>
        public object m_filterCookie;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 41;

        #endregion
    }
}