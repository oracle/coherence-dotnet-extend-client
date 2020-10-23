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

import com.tangosol.net.Invocable;
import com.tangosol.net.InvocationService;


public class EmptyInvocable
        implements Invocable, PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public EmptyInvocable()
        {
        super();
        }


    // ----- Invocable interface --------------------------------------------

    public void init(InvocationService service)
        {
        }

    public void run()
        {
        }

    public Object getResult()
        {
        return new Integer(42);
        }


    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader in)
            throws IOException
        {
        }

    public void writeExternal(PofWriter out)
            throws IOException
        {
        }
    }
