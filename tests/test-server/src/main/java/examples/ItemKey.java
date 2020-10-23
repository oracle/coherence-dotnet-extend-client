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

import com.tangosol.net.cache.KeyAssociation;

import java.io.IOException;


public class ItemKey
        implements PortableObject, KeyAssociation
    {
    // ----- constructors ---------------------------------------------------

    public ItemKey()
        {
        }

    public ItemKey(int id, int parentId)
        {
        this.m_id = id;
        this.m_parentId  = parentId;
        }

	public Object getAssociatedKey()
		{
		return new Integer(m_parentId);
		}

    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader)
            throws IOException
        {
        m_id        = reader.readInt(0);
        m_parentId  = reader.readInt(1);
        }

    public void writeExternal(PofWriter writer)
            throws IOException
        {
        writer.writeInt(0, m_id);
        writer.writeInt(1, m_parentId);
        }


    // ----- accessors ------------------------------------------------------

    public int getId()
        {
        return m_id;
        }

    public void setId(int id)
        {
        this.m_id = id;
        }
		
	public int getParentId()
        {
        return m_parentId;
        }

    public void setParentId(int parentId)
        {
        this.m_parentId = parentId;
        }

    // ----- data members ---------------------------------------------------

	private int m_id;

	private int m_parentId;
    }