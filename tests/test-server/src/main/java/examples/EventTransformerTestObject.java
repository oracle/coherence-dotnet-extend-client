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

public class EventTransformerTestObject implements PortableObject 
    {
    private int ID;
    private String name;

    public EventTransformerTestObject()
        {}
    
    public int getID() 
        {
        return ID;
        }

    public void setID(int iD)
        {
        ID = iD;
        }

    public String getName() 
        {
        return name;
        }

    public void setName(String name) 
        {
        this.name = name;
        }

    @Override
    public String toString() 
        {
        return "EventTransformerTestObject [ID=" + ID + ", name=" + name + "]";
        }

    @Override
    public int hashCode()
        {
        final int prime = 31;
        int result = 1;
        result = prime * result + ID;
        result = prime * result + ((name == null) ? 0 : name.hashCode());
        return result;
        }

    @Override
    public boolean equals(Object obj)
        {
        if (this == obj)
            return true;
        if (obj == null)
            return false;
        if (getClass() != obj.getClass())
            {
            return false;
            }
        EventTransformerTestObject that = (EventTransformerTestObject) obj;
        if (ID != that.ID)
            {
            return false;
            }
        if (name == null)
            {
            if (that.name != null)
                {
                return false;
                } 
            }
        else if (!name.equals(that.name))
            {
            return false;
            }
        return true;
        }

    public void readExternal(PofReader reader) throws IOException 
        {
        this.ID = reader.readInt(0);
        this.name = reader.readString(1);
        }

    public void writeExternal(PofWriter writer) throws IOException 
        {
        writer.writeInt(0, ID);
        writer.writeString(1, name);
        }
    }
