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

import com.tangosol.net.CacheFactory;
import com.tangosol.net.Invocable;
import com.tangosol.net.InvocationService;

import java.io.IOException;


/**
* Invocable implementation that stops the proxy service.
*
* @author par  2012.12.12
*/
public class ProxyStopInvocable
        implements Invocable, PortableObject
    {
    // ----- constructors ---------------------------------------------------

    /**
    * Default constructor.
    */
    public ProxyStopInvocable()
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
        String sService = getProxyServiceName();
        if (CacheFactory.getCluster().getService(sService) != null)
            {
            // go through the "front door" to get the SafeService
            CacheFactory.getService(sService).shutdown();
            }
        CacheFactory.getService(sService).start();
        }

    /**
    * {@inheritDoc}
    */
    public Object getResult()
        {
        return new String("Test");
        }


    // ----- PortableObject interface ---------------------------------------

    /**
    * {@inheritDoc}
    */
    public void readExternal(PofReader in)
            throws IOException
        {
        setProxyServiceName(in.readString(0));
        }

    /**
    * {@inheritDoc}
    */
    public void writeExternal(PofWriter out)
            throws IOException
        {
        out.writeString(0, getProxyServiceName());
        }


    // ----- accessors ------------------------------------------------------

    /**
    * Return the proxy service to stop.
    *
    * @return the proxy service name
    */
    public String getProxyServiceName()
        {
        return m_proxyServiceName;
        }

    /**
    * Configure the proxy service to stop.
    *
    * @param name  the proxy service to stop
    */
    public void setProxyServiceName(String name)
        {
        m_proxyServiceName = name;
        }


    // ----- data members ---------------------------------------------------

    /**
    * The service name to stop.
    */
    protected String m_proxyServiceName;
    }
