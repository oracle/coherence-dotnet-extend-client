/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Net.Messaging.Impl.NameService
{
    /// <summary>
    /// The InvocationRequest is an <see cref="NameServiceRequest"/>
    /// that has the NameService.lookup(String sName) Request message.
    /// </summary>
    /// <author>Wei Lin  2012.05.23</author>
    /// <since>Coherence 12.1.2</since>
    /// <seealso cref="NameServiceRequest"/>
    public class LookupRequest : NameServiceRequest
    {
        #region Properties

        /// <summary>
        /// Return the type identifier for this <b>Message</b>.
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
        /// The name to look up.
        /// </summary>
        /// <value>
        /// The name to look up.
        /// </value>
        public virtual string LookupName
        {
            get; set;
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

            writer.WriteString(1, LookupName);
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

            LookupName = (string)reader.ReadString(1);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The type identifier for this <b>Message</b> class.
        /// </summary>
        public const int TYPE_ID = 1;

        #endregion
    }
}
