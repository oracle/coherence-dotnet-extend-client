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


public class Order
        implements PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public Order()
        {
        }

    public Order(int orderId, String name)
        {
        this.m_orderId = orderId;
        this.m_name   = name;
        }


    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader)
            throws IOException
        {
        m_orderId = reader.readInt(0);
        m_name    = reader.readString(1);
        }

    public void writeExternal(PofWriter writer)
            throws IOException
        {
        writer.writeInt(0, this.m_orderId);
        writer.writeString(1, this.m_name);
        }


    // ----- accessors ------------------------------------------------------

    public int getOrderId()
        {
        return m_orderId;
        }

    public void setOrderId(int orderId)
        {
        this.m_orderId = orderId;
        }
		
	public String getName()
        {
        return m_name;
        }

    public void setName(String name)
        {
        this.m_name = name;
        }

    // ----- data members ---------------------------------------------------

	private int m_orderId;
    
	private String m_name;	
    }