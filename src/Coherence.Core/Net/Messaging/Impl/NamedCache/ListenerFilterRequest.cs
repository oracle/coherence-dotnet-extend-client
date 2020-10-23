/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The ListenerFilterRequest is a <see cref="FilterRequest"/> sent to
    /// register or deregister interest in events that pass a specified
    /// <see cref="IFilter"/> on a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="IFilter"/>
    /// <seealso cref="FilterRequest"/>
    public class ListenerFilterRequest : FilterRequest
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
        /// <b>true</b> to add an <see cref="ICacheListener"/>,
        /// <b>false</b> to remove <b>ICacheListener</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> to add an <b>ICacheListener</b>, <b>false</b> to
        /// remove <b>ICacheListener</b>.
        /// </value>
        public virtual bool Add
        {
            get { return m_add; }
            set { m_add = value; }
        }

        /// <summary>
        /// A unique identifier for the <see cref="IFilter"/> associated with
        /// this ListenerFilterRequest.
        /// </summary>
        /// <value>
        /// <b>IFilter</b> unique identifier.
        /// </value>
        public virtual long FilterId
        {
            get { return m_filterId; }
            set { m_filterId = value; }
        }

        /// <summary>
        /// An optional <see cref="ICacheTrigger"/> object associated with
        /// this request.
        /// </summary>
        /// <value>
        /// An <b>ICacheTrigger</b> object associated with this request.
        /// </value>
        public virtual ICacheTrigger Trigger
        {
            get { return m_trigger; }
            set { m_trigger = value; }
        }

        /// <summary>
        /// <b>true</b> if the <b>ICacheListener</b> is "lite",
        /// <b>false</b> if it is a standard <b>ICacheListener</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <b>ICacheListener</b> is "lite",
        /// <b>false</b> if it is a standard <b>ICacheListener</b>.
        /// </value>
        public virtual bool IsLite
        {
            get { return m_isLite; }
            set { m_isLite = value; }
        }

        /// <summary>
        /// <b>true</b> if the <b>ICacheListener</b> is "priming",
        /// <b>false</b> if it is a non-priming <b>ICacheListener</b>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <b>ICacheListener</b> is "priming",
        /// <b>false</b> if it is a non-priming <b>ICacheListener</b>.
        /// </value>
        /// <since>12.2.1</since>
        public virtual bool IsPriming
        {
            get { return m_isPriming; }
            set { m_isPriming = value; }
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

            FilterId = reader.ReadInt64(2);
            Add      = reader.ReadBoolean(3);
            IsLite   = reader.ReadBoolean(4);
            Trigger  = (ICacheTrigger) reader.ReadObject(5);
            if (ImplVersion > 5)
            {
                IsPriming = reader.ReadBoolean(6);
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
        /// <seealso cref="Request.WriteExternal"/>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteInt64(2, FilterId);
            writer.WriteBoolean(3, Add);
            writer.WriteBoolean(4, IsLite);
            writer.WriteObject(5, Trigger);
            if (ImplVersion > 5)
            {
                writer.WriteBoolean(6, IsPriming);
            }
        }

        #endregion
         
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription()
                   + ", FilterId=" + FilterId
                   + ", Add="      + Add
                   + ", Lite="     + IsLite
                   + ", Trigger="  + Trigger;
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// <b>true</b> to add an ICacheListener, <b>false</b> to remove
        /// an ICacheListener.
        /// </summary>
        private bool m_add;

        /// <summary>
        /// A unique identifier for the IFilter associated with this
        /// <b>ListenerFilterRequest</b>.
        /// </summary>
        private long m_filterId;

        /// <summary>
        /// <b>true</b> if the ICacheListener is "lite", <b>false</b> if
        /// it is a standard ICacheListener.
        /// </summary>
        private bool m_isLite;

        /// <summary>
        /// An optional ICacheTrigger object associated with this request.
        /// </summary>
        private ICacheTrigger m_trigger;

        /// <summary>
        /// Support for the NearCache priming listener. The value of true 
        /// indicates that the listener registration should force a synthetic 
        /// event containing the current value to the requesting client. This 
        /// property was added to Coherence 12.2.1 (protocol version 6) for 
        /// COH-4615 implementation.
        /// </summary>
        /// <since>12.2.1</since>
        private bool m_isPriming;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 12;

        #endregion
    }
}