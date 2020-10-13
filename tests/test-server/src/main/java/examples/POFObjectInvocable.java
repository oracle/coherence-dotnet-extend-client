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


/**
* Invocable implementation that serializes and deserializes a POF object
* between .net client and server
*
* @author Wei Lin 8/24/2010
*/
public class POFObjectInvocable
        implements Invocable, PortableObject
    {
    // ----- constructors ---------------------------------------------------

    /**
    * constructor
    */
    public POFObjectInvocable()
        {
        super();
        }


    // ----- Invocable interface --------------------------------------------

    /**
    * {@inheritDoc}
    */
    public void init(InvocationService service)
        {
        }

    /**
    * {@inheritDoc}
    */
    public void run()
        {
        }

    /**
    * {@inheritDoc}
    */
    public Object getResult()
        {
        return m_oPofObject;
        }


    // ----- PortableObject interface ---------------------------------------

    /**
    * {@inheritDoc}
    */
    public void readExternal(PofReader in)
            throws IOException
        {
        m_oPofObject = in.readObject(0);
        }

    /**
    * {@inheritDoc}
    */
    public void writeExternal(PofWriter out)
            throws IOException
        {
        out.writeObject(0, m_oPofObject);
        }


    // ----- accessors ------------------------------------------------------

    /**
    * Set the POF object
    *
    * @param o  the new value of the POF object
    */
    public void setPofObject(Object o)
        {
        m_oPofObject = o;
        }

    /**
    * Return the POF object.
    *
    * @return POF object
    */
    public Object getPofObject()
        {
        return m_oPofObject;
        }


    // ----- data members ---------------------------------------------------

    /**
    * The POF object.
    */
    private Object m_oPofObject;
}
