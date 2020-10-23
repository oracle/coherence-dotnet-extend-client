/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;


import java.io.IOException;

import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;
import com.tangosol.io.pof.PortableObject;

import com.tangosol.util.Versionable;


public class Temperature
        implements Versionable, PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public Temperature()
        {
        super();
        }

    public Temperature(int nValue, char chGrade, int cHours)
        {
        m_nValue = nValue;
        if (chGrade == 'c' || chGrade == 'C')
            {
            m_chGrade = 'C';
            }
        else if (chGrade == 'f' || chGrade == 'F')
            {
            m_chGrade = 'F';
            }
        else
            {
            throw new IllegalArgumentException();
            }
        m_nVersion = cHours % 24;
        }


    // ----- Versionable interface ------------------------------------------

    public void incrementVersion()
        {
        m_nVersion = (m_nVersion + 1) % 24;
        }

    public Comparable getVersionIndicator()
        {

        return new Integer(m_nVersion);
        }


    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader in)
            throws IOException
        {
        m_nValue   = in.readInt(0);
        m_chGrade  = in.readChar(1);
        m_nVersion = in.readInt(2);
        }

    public void writeExternal(PofWriter out)
            throws IOException
        {
        out.writeInt (0, m_nValue);
        out.writeChar(1, m_chGrade);
        out.writeInt (2, m_nVersion);
        }


    // ----- accessors ------------------------------------------------------

    public char getGrade()
        {
        return m_chGrade;
        }

    public void setGrade(char grade)
        {
        if (grade == 'c' || grade == 'C')
            {
            m_chGrade = 'C';
            }
        else if (grade == 'f' || grade == 'F')
            {
            m_chGrade = 'F';
            }
        else
            {
            throw new IllegalArgumentException();
            }
        }

    public int getValue()
        {
        return m_nValue;
        }

    public void setValue(int value)
        {
        m_nValue = value;
        }

     public int getVersion()
        {
        return m_nVersion;
        }

    public void setVersion(int v)
        {
        m_nVersion = v % 24;
        }


    // ----- data members ---------------------------------------------------

    private char m_chGrade;

    private int m_nValue;

    private int m_nVersion;
    }