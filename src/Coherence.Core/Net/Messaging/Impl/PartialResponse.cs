/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// Abstract <see cref="IResponse"/> implementation that carries a
    /// partial result.
    /// </summary>
    public abstract class PartialResponse : Response
    {
        #region Properties
        
        /// <summary>
        /// Opaque cookie used to support streaming.
        /// </summary>
        /// <remarks>
        /// If non-null, this PartialResponse contains a partial result. The
        /// receiver of a PartialResponse can accumulate or iterate the
        /// entire result by sending additional Request(s) until this
        /// property is <c>null</c>.
        /// </remarks>
        /// <value>
        /// <see cref="Binary"/> representing cookie used to support
        /// streaming.
        /// </value>
        public virtual Binary Cookie
        {
            get { return m_cookie; }
            set { m_cookie = value; }
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
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            Cookie = reader.ReadBinary(6);
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

            writer.WriteBinary(6, Cookie);
        }

        #endregion
        
        #region Extend Overrides

        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", Cookie=" + Cookie;
        }

        #endregion

        #region Data members

        /// <summary>
        /// Opaque cookie used to support streaming.
        /// </summary>
        private Binary m_cookie;

        #endregion
    }
}
