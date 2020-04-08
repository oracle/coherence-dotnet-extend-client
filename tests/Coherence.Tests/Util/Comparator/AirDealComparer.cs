/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Util.Comparator
{
    /// <summary>
    /// IComparer implementation that performs a comparison of
    /// two AirDeals (custom type).
    /// </summary>
    /// <author>lh  2002.02.08</author>
    public class AirDealComparer : IComparer, IPortableObject
    {
        #region Comparator interface

        /// <summary>
        /// Compare two AirDeals.
        /// </summary>
        public int Compare(object o1, object o2)
        {
            var airDeal1 = (AirDeal) o1;
            var airDeal2 = (AirDeal) o2;
            return (int) (airDeal1.DealAppeal - airDeal2.DealAppeal);
        }

        #endregion

        #region PortableObject interface

        /// <summary>
        /// Restore the contents of a AirDealComparer instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public void ReadExternal(IPofReader reader)
        {
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
        public void WriteExternal(IPofWriter writer)
        {
        }
        #endregion

        #region Inner Class: AirDeal

        public class AirDeal : IPortableObject
        {
            #region Properties

            public long Oid
            {
                get { return m_oid; }
                set { m_oid = value; }
            }

            public String OrigAirport
            {
                get { return m_origAirport; }
                set { m_origAirport = value; }
            }

            public String DestAirport
            {
                get { return m_destAirport; }
                set { m_destAirport = value; }
            }

            public double DealAppeal
            {
                get { return m_dealAppeal; }
                set { m_dealAppeal = value; }
            }

            #endregion

            #region Constructors

            public AirDeal()
            {
            }

            public AirDeal(long oid, String oringAirport, String destAirport, double dealAppeal)
            {
                Oid         = oid;
                OrigAirport = oringAirport;
                DestAirport = destAirport;
                DealAppeal  = dealAppeal;
            }

            #endregion

            #region PortableObject interface

            /// <summary>
            /// Restore the contents of a AirDealComparer instance by reading its state
            /// using the specified <see cref="IPofReader"/> object.
            /// </summary>
            /// <param name="reader">
            /// The <b>IPofReader</b> from which to read the object's state.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public void ReadExternal(IPofReader reader)
            {
                Oid         = reader.ReadInt64(OID);
                OrigAirport = reader.ReadString(ORIGAIRPORT);
                DestAirport = reader.ReadString(DESTAIRPORT);
                DealAppeal  = reader.ReadDouble(DEALAPPEAL);
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
            public void WriteExternal(IPofWriter writer)
            {
                writer.WriteInt64(OID, m_oid);
                writer.WriteString(ORIGAIRPORT, m_origAirport);
                writer.WriteString(DESTAIRPORT, m_destAirport);
                writer.WriteDouble(DEALAPPEAL, m_dealAppeal);
            }

            #endregion

            #region Constants and members

            private const int OID         = 0;
            private const int ORIGAIRPORT = 1;
            private const int DESTAIRPORT = 2;
            private const int DEALAPPEAL  = 3;

            private long   m_oid;
            private String m_origAirport;
            private String m_destAirport;
            private double m_dealAppeal;

            #endregion
        }
        #endregion
    }
}