/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The ContainsValueRequest is a <see cref="NamedCacheRequest"/> sent to
    /// determine if a remote NamedCache contains a mapping for a specific
    /// value.
    /// </summary>
    /// <author>Ivan Cikic  2006.08.30</author>
    /// <seealso cref="NamedCacheRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class ContainsValueRequest : NamedCacheRequest
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
        /// The entry value for which mapping is searched in NamedCache.
        /// </summary>
        /// <value>
        /// The entry value for which mapping is searched in NamedCache.
        /// </value>
        public virtual object Value
        {
            get { return m_value; }
            set { m_value = value; }
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

            Value = reader.ReadObject(1);
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

            writer.WriteObject(1, Value);

            // release state
            Value = null;
        }

        #endregion
        
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            object oValue = Value;
            
            return base.GetDescription() + ", Value=" + (oValue == null ? "null" : oValue.GetType().Name + "(HashCode=" + oValue.GetHashCode() + ')');
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 3;

        /// <summary>
        /// The entry value.
        /// </summary>
        private object m_value;

        #endregion
    }
}