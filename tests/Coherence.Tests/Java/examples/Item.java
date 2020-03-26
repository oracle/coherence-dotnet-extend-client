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


public class Item
        implements PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public Item()
        {
        }

    public Item(int itemId, double sum)
        {
        this.m_itemId = itemId;
        this.m_sum   = sum;
        }


    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader)
            throws IOException
        {
        m_itemId = reader.readInt(0);
        m_sum    = reader.readDouble(1);
        }

    public void writeExternal(PofWriter writer)
            throws IOException
        {
        writer.writeInt(0, this.m_itemId);
        writer.writeDouble(1, this.m_sum);
        }


    // ----- accessors ------------------------------------------------------

    public int getItemId()
        {
        return m_itemId;
        }

    public void setItemId(int itemId)
        {
        this.m_itemId = itemId;
        }
		
	public double getSum()
        {
        return m_sum;
        }

    public void setSum(double sum)
        {
        this.m_sum = sum;
        }

    // ----- data members ---------------------------------------------------

	private int m_itemId;
    
	private double m_sum;	
    }