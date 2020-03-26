/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The PutAllRequest is a <see cref="NamedCacheRequest"/> sent to update
    /// one or more mappings in a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.08.31</author>
    /// <seealso cref="NamedCacheRequest"/>
    /// <seealso cref="NamedCacheProtocol"/>
    public class PutAllRequest : NamedCacheRequest
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
        /// The map of entries to be updated when this message is processed.
        /// </summary>
        /// <value>
        /// The map of entries to be updated when this message is processed.
        /// </value>
        public virtual IDictionary Map
        {
            get { return m_map; }
            set { m_map = value; }
        }

        #endregion

        #region IPortableObject implementation

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

            writer.WriteDictionary(1, Map);

            // release state
            Map = null;
        }

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

            Map = reader.ReadDictionary(1, new HashDictionary());
        }

        #endregion
        
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            IDictionary map = Map;
            
            string sMapDesc = map == null ? "null" :  "Count=" + map.Count + ", HashCode=" + map.GetHashCode();
            
            return base.GetDescription() +  ", Dictionary=(" + sMapDesc + ')';
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 7;

        /// <summary>
        /// The map of entries to be updated when this message is
        /// processed.
        /// </summary>
        private IDictionary m_map;

        #endregion
    }
}
