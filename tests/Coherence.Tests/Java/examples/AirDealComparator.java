/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import com.tangosol.io.pof.PortableObject;
import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;

import java.io.IOException;

import java.util.Comparator;

/**
 * Comparator implementation that performs a comparison of
 * two AirDeals (custom type).
 *
 * @author lh  2012.09.05
 */
public class AirDealComparator implements Comparator<AirDealComparator.AirDeal>, PortableObject
    {
    // ----- constructors ---------------------------------------------------

    /**
     * Default constructor, create a new AirDealComparator.
     */
    public AirDealComparator()
        {
        }

    // ----- Comparator interface -------------------------------------------

    /**
     * Compare two AirDeals.
     */
    public int compare(AirDeal airDeal1, AirDeal airDeal2)
        {
        return (int) (airDeal1.getDealAppeal() - airDeal2.getDealAppeal());
        }

    // ----- PortableObject interface ---------------------------------------

    /**
     * {@inheritDoc}
     */
    public void readExternal(PofReader reader)
        throws IOException
        {
        }

    /**
     * {@inheritDoc}
     */
    public void writeExternal(PofWriter writer)
        throws IOException
        {
        }

    public static class AirDeal implements PortableObject
        {
        private long   m_oid;
        private String m_origAirport;
        private String m_destAirport;
        private double m_dealAppeal;

        // ----- constructors -------------------------------------------

        /**
         * Default constructor, create a new AirDeal.
         */
        public AirDeal()
            {
            }

        public AirDeal(long oid, String oringAirport, String destAirport, double dealAppeal)
            {
            m_oid         = oid;
            m_origAirport = oringAirport;
            m_destAirport = destAirport;
            m_dealAppeal  = dealAppeal;
            }        

        // ----- accessors ----------------------------------------------

        public long getOid()
            {
            return m_oid;
            }

        public String getOrigAirport()
            {
            return m_origAirport;
            }

        public String getDestAirport()
            {
            return m_destAirport;
            }

        public double getDealAppeal()
            {
            return m_dealAppeal;
            }

        // ----- PortableObject interface -------------------------------

        /**
         * {@inheritDoc}
         */
        public void readExternal(PofReader reader)
            throws IOException
            {
            m_oid         = reader.readLong(OID);
            m_origAirport = reader.readString(ORIGAIRPORT);
            m_destAirport = reader.readString(DESTAIRPORT);
            m_dealAppeal  = reader.readDouble(DEALAPPEAL);
            }

        /**
         * {@inheritDoc}
         */
        public void writeExternal(PofWriter writer)
            throws IOException
            {
            writer.writeLong(OID, m_oid);
            writer.writeString(ORIGAIRPORT, m_origAirport);
            writer.writeString(DESTAIRPORT, m_destAirport);
            writer.writeDouble(DEALAPPEAL, m_dealAppeal);
            }

        // ----- constants ----------------------------------------------

        private static final int OID         = 0;
        private static final int ORIGAIRPORT = 1;
        private static final int DESTAIRPORT = 2;
        private static final int DEALAPPEAL  = 3;
        }
    }