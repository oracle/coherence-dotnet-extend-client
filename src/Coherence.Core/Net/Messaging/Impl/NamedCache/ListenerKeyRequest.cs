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
    /// The ListenerKeyRequest is a <see cref="KeyRequest"/> sent to register
    /// or deregister interest in events that pass a specified
    /// <see cref="IFilter"/> on a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="KeyRequest"/>
    public class ListenerKeyRequest : KeyRequest
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

            Add     = reader.ReadBoolean(2);
            IsLite  = reader.ReadBoolean(3);
            Trigger = (ICacheTrigger) reader.ReadObject(4);
            if (ImplVersion > 5)
            {
                IsPriming = reader.ReadBoolean(5);
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

            writer.WriteBoolean(2, Add);
            writer.WriteBoolean(3, IsLite);
            writer.WriteObject(4, Trigger);
            if (ImplVersion > 5)
            {
                writer.WriteBoolean(5, IsPriming);
            }
        }

        #endregion
        
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription()
                   + ", Add="      + Add
                   + ", Lite="     + IsLite
                   + ", Trigger="  + Trigger
                   + ", Priming="  + IsPriming;
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// <b>true</b> to add an ICacheListener, <b>false</b> to remove
        /// an ICacheListener.
        /// </summary>
        private bool m_add;

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
        public const int TYPE_ID = 11;

        #endregion
    }
}
