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

public class SimpleAddressComparator
        implements Comparator, PortableObject
    {
    // ----- Comparator interface ---------------------------------------
    
    public int compare(Object o1, Object o2)
        {
        if (o1 == null)
            {
            return o2 == null ? 0 : -1;
            }

        if (o2 == null)
            {
            return 1;
            }
            
        if (((Address)o1).getZip() == null)
            {
            return (((Address)o2).getZip() == null ? 0 : -1);
            }
 
        return ((Address)o1).getZip().compareTo(((Address)o2).getZip());
        }

        
    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader)
            throws IOException
        {
        }

    public void writeExternal(PofWriter writer)
            throws IOException
        {
        }    
    }