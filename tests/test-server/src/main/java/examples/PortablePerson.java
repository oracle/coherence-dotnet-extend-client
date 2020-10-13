/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;
import com.tangosol.io.pof.PortableObject;

import java.io.IOException;
import java.util.Date;

public class PortablePerson
    implements PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public PortablePerson()
        {
        }

    public PortablePerson(String sName, Date dtDOB)
        {
        m_sName = sName;
        m_dtDOB = dtDOB;
        }

    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader)
        throws IOException
        {
        m_sName = reader.readString(NAME);
        setAddress((Address) reader.readObject(ADDRESS));
        m_dtDOB = reader.readDate(DOB);
        setSpouse((PortablePerson) reader.readObject(SPOUSE));
        setChildren((PortablePerson[]) reader.readObjectArray(CHILDREN,
                                                      new PortablePerson[0]));
        }

    public void writeExternal(PofWriter writer)
        throws IOException
        {
        writer.writeString(NAME, m_sName);
        writer.writeObject(ADDRESS, getAddress());
        writer.writeDateTime(DOB, m_dtDOB);
        writer.writeObject(SPOUSE, getSpouse());
        writer.writeObjectArray(CHILDREN, getChildren(), PortablePerson.class);
        }

    // ----- accessors ------------------------------------------------------

    public Address getAddress()
        {
        return m_addr;
        }

    public void setAddress(Address addr)
        {
        m_addr = addr;
        }

    public PortablePerson getSpouse()
        {
        return m_spouse;
        }

    public void setSpouse(PortablePerson spouse)
        {
        m_spouse = spouse;
        }

    public PortablePerson[] getChildren()
        {
        return m_aChildren;
        }

    public void setChildren(PortablePerson[] aChildren)
        {
        m_aChildren = aChildren;
        }

    // ----- data members ---------------------------------------------------

    private String           m_sName;
    private Address          m_addr;
    private Date             m_dtDOB;
    private PortablePerson   m_spouse;
    private PortablePerson[] m_aChildren;

    public static final int NAME     = 0;
    public static final int ADDRESS  = 1;
    public static final int DOB      = 2;
    public static final int SPOUSE   = 3;
    public static final int CHILDREN = 4;
    }