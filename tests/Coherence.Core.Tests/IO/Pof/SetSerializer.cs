/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Util.Collections;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// A POF serializer for HashSet.
    /// </summary>
    /// <author>lh  2011.02.04</author>
    /// <since>Coherence 3.7</since>
    public class SetSerializer : IPofSerializer
    {
        #region IPofSerializer implementation

        /// <see cref="IPofSerializer"/>
        object IPofSerializer.Deserialize(IPofReader reader)
        {
            var set = new HashSet();
            reader.RegisterIdentity(set);
            reader.ReadCollection(0, set);
            reader.ReadRemainder();
            return set;
        }

        /// <see cref="IPofSerializer"/>
        void IPofSerializer.Serialize(IPofWriter writer, object o)
        {
            writer.WriteCollection(0, (HashSet) o);
            writer.WriteRemainder(null);
        }

        #endregion
    }
}
