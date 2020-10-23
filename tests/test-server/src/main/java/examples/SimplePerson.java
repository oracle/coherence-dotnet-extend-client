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

import com.tangosol.util.ImmutableArrayList;
import com.tangosol.util.ListMap;

import java.io.IOException;

import java.util.ArrayList;
import java.util.Calendar;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Random;


/**
 * Test class describing a person with simple data fields.
 *
 * @author par 9/25/2013
 *
 * @since @BUILDVERSION@
 */
public class SimplePerson
        implements PortableObject
    {
    // ----- constructors ---------------------------------------------------

    /**
    * Default constructor for Externalizable.
    */
    public SimplePerson()
        {}

    public SimplePerson(String sSSN, String sFirst, String sLast, int nYear,
                  String sMotherId, String[] asChildrenId)
        {
        m_sSSN         = sSSN;
        m_sFirstName   = sFirst;
        m_sLastName    = sLast;
        m_nYear        = nYear;
        m_sMotherSSN   = sMotherId;
        m_listChildren = new ImmutableArrayList(asChildrenId);
        }

    // ----- Accessors ------------------------------------------------------

    public String getLastName()
        {
        return m_sLastName;
        }

    public void setLastName(String sLast)
        {
        m_sLastName = sLast;
        }
     
    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader in)
            throws IOException
        {
        m_sSSN         = in.readString(SSN);
        m_sFirstName   = in.readString(FIRST_NAME);
        m_sLastName    = in.readString(LAST_NAME);
        m_nYear        = in.readInt(BIRTH_YEAR);
        m_sMotherSSN   = in.readString(MOTHER_SSN);
        m_listChildren = (List) in.readCollection(CHILDREN, new ArrayList());
        }

    public void writeExternal(PofWriter out)
            throws IOException
        {
        out.writeString(SSN, m_sSSN);
        out.writeString(FIRST_NAME, m_sFirstName);
        out.writeString(LAST_NAME, m_sLastName);
        out.writeInt(BIRTH_YEAR, m_nYear);
        out.writeString(MOTHER_SSN, m_sMotherSSN);
        out.writeCollection(CHILDREN, m_listChildren);
        }

    // ----- data members ---------------------------------------------------

    private String m_sSSN;
    private String m_sFirstName = "";
    private String m_sLastName  = "";
    private int    m_nYear;
    private String m_sMotherSSN = "";
    private List   m_listChildren = new ArrayList();
 
    // ----- POF index constants --------------------------------------------

    public static final int SSN        = 0;
    public static final int FIRST_NAME = 1;
    public static final int LAST_NAME  = 2;
    public static final int BIRTH_YEAR = 3;
    public static final int MOTHER_SSN = 4;
    public static final int CHILDREN   = 5;
    }