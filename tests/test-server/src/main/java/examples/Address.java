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


public class Address
        implements PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public Address()
        {
        }

    public Address(String sStreet, String sCity, String sState, String sZip)
        {
        this.m_sStreet = sStreet;
        this.m_sCity   = sCity;
        this.m_sState  = sState;
        this.m_sZip    = sZip;
        }


    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader)
            throws IOException
        {
        m_sStreet = reader.readString(0);
        m_sCity   = reader.readString(1);
        m_sState  = reader.readString(2);
        m_sZip    = reader.readString(3);
        }

    public void writeExternal(PofWriter writer)
            throws IOException
        {
        writer.writeString(0, this.m_sStreet);
        writer.writeString(1, this.m_sCity);
        writer.writeString(2, this.m_sState);
        writer.writeString(3, this.m_sZip);
        }


    // ----- accessors ------------------------------------------------------

    public String getStreet()
        {
        return m_sStreet;
        }

    public void setStreet(String sStreet)
        {
        this.m_sStreet = sStreet;
        }

    public String getCity()
        {
        return m_sCity;
        }

    public void setCity(String sCity)
        {
        this.m_sCity = sCity;
        }

    public String getState()
        {
        return m_sState;
        }

    public void setState(String sState)
        {
        this.m_sState = sState;
        }

    public String getZip()
        {
        return m_sZip;
        }

    public void setZip(String sZip)
        {
        this.m_sZip = sZip;
        }


    // ----- data members ---------------------------------------------------

    private String m_sStreet;

    private String m_sCity;

    private String m_sState;

    private String m_sZip;
    }