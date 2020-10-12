/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;
using System.Security.Principal;

using Tangosol.Net.Security.Impl;

namespace Tangosol.IO.Pof
{
    /// <summary><see cref="IPofSerializer"/> implementation that supports
    /// the serialization and deserialization of an <see cref="IPrincipal"/> 
    /// to and from a POF stream.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Only the <b>IIdentity</b> associated with the <b>IPrincipal</b> is
    /// serialized. All role information encapsulated by the <b>IPrincipal</b>
    /// is considered transient.</p>
    /// <p>
    /// The <b>PrincipalPofSerializer</b> can serialize any <b>IPrincipal</b> 
    /// implementation to a POF stream; however, the <b>IPrincipal</b> returned
    /// during deserialization is always an instance of 
    /// <see cref="SimplePrincipal"/> with an empty role array.</p>
    /// </remarks>
    /// <author>Jason Howes  2008.08.12</author>
    public class PrincipalPofSerializer : IPofSerializer
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PrincipalPofSerializer()
        {
        }

        #endregion

        #region IPofSerializer implementation

        /// <summary>
        /// Serialize a user type instance to a POF stream by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for writing out an object of a
        /// user type:
        /// <list type="number">
        /// <item>
        /// <description>
        /// If the object is evolvable, the implementation must set the
        /// version by calling <see cref="IPofWriter.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may write any combination of the properties of
        /// the user type by using the "write" methods of the
        /// <b>IPofWriter</b>, but it must do so in the order of the property
        /// indexes.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// After all desired properties of the user type have been written,
        /// the implementation must terminate the writing of the user type by
        /// calling <see cref="IPofWriter.WriteRemainder"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="writer">
        /// The <b>IPofWriter</b> with which to write the object's state.
        /// </param>
        /// <param name="o">
        /// The object to serialize.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void Serialize(IPofWriter writer, object o)
        {
            IPrincipal  principal  = (IPrincipal) o;
            IList       identities = new ArrayList(1);

            identities.Add(principal.Identity);

            writer.WriteCollection(0, identities);
            writer.WriteRemainder(null);
        }

        /// <summary>
        /// Deserialize a user type instance from a POF stream by reading its
        /// state using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <remarks>
        /// An implementation of <b>IPofSerializer</b> is required to follow
        /// the following steps in sequence for reading in an object of a
        /// user type:
        /// <list type="number">
        /// <item>
        /// <description>
        /// If the object is evolvable, the implementation must get the
        /// version by calling <see cref="IPofWriter.VersionId"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// The implementation may read any combination of the
        /// properties of the user type by using "read" methods of the
        /// <b>IPofReader</b>, but it must do so in the order of the property
        /// indexes.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// After all desired properties of the user type have been read,
        /// the implementation must terminate the reading of the user type by
        /// calling <see cref="IPofReader.ReadRemainder"/>.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="reader">
        /// The <b>IPofReader</b> with which to read the object's state.
        /// </param>
        /// <returns>
        /// The deserialized user type instance.
        /// </returns>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public object Deserialize(IPofReader reader)
        {
            IList identities = (IList) reader.ReadCollection(0, new ArrayList(1));
            reader.ReadRemainder();

            IPrincipal principal = null;
            if (identities.Count > 0)
            {
                IIdentity identity = (IIdentity) identities[0];
                principal = new SimplePrincipal(identity, null);
                reader.RegisterIdentity(principal);
            }

            return principal;
        }

        #endregion
    }
}