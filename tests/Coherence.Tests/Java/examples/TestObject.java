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

import com.tangosol.util.Base;

import java.io.IOException;


public class TestObject 
        implements PortableObject
    {

    // ----- constructors ---------------------------------------------------

    public TestObject()
        {	
        }

    public TestObject(int iD, String name)
        {
        this.m_ID = iD;
        this.m_name = name;
        }


    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader)
            throws IOException
        {
        this.m_ID   = reader.readInt(0);
        this.m_name = reader.readString(1);
        }

    public void writeExternal(PofWriter writer)
            throws IOException
        {
        writer.writeInt(0, m_ID);
        writer.writeString(1, m_name);
        }

    // ----- accessors ------------------------------------------------------

    public int getID()
        {
        return m_ID;
        }

    public void setID(int iD)
        {
        this.m_ID = iD;
        }

    public String getName()
        {
        return m_name;
        }

    public void setName(String name)
        {
        this.m_name = name;
        }

    // ----- Object --------------------------------------------------------

    @Override
    public String toString()
        {
        return "TestObject [ID=" + m_ID + ", name=" + m_name + "]";
	}

    @Override
    public int hashCode()
        {
        final int prime = 31;
        int result = 1;
        result = prime * result + m_ID;
        result = prime * result + ((m_name == null) ? 0 : m_name.hashCode());
        return result;
	}

    @Override
    public boolean equals(Object obj)
        {
        if (this == obj)
            {
            return true;
            }

        if (obj == null)
            {
            return false;
            }

        if (getClass() != obj.getClass())
            {
            return false;
            }

        TestObject other = (TestObject) obj;
        if (m_ID != other.m_ID)
            {
            return false;
            }
        if (m_name == null)
            {	
            if (other.m_name != null)
                {
                return false;
                }
            }
        return Base.equals(m_name, other.m_name);
	}

    // ----- data members ---------------------------------------------------

    private int    m_ID;

    private String m_name;
    }
