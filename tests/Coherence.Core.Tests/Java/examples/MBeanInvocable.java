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
import com.tangosol.net.Cluster;
import com.tangosol.net.Invocable;
import com.tangosol.net.InvocationService;

import com.tangosol.net.management.MBeanHelper;
import com.tangosol.net.management.Registry;

import com.tangosol.util.Base;

import java.io.IOException;

import java.util.Iterator;
import java.util.Set;

import javax.management.MBeanServer;
import javax.management.ObjectInstance;
import javax.management.ObjectName;


/**
* Invocable implementation that queries the Member attribute of the
* ConnectionMBean and returns the string value.
*
* @author lh  2010.01.24
*/
public class MBeanInvocable
        implements Invocable, PortableObject
    {
    // ----- constructors ---------------------------------------------------

    /**
    * Default constructor.
    */
    public MBeanInvocable()
        {
        }


    // ----- Invocable interface --------------------------------------------

    /**
    * {@inheritDoc}
    */
    public void init(InvocationService service)
        {
        assert service.getInfo().getServiceType()
                .equals(InvocationService.TYPE_REMOTE);
        m_service = service;
        }

    /**
    * {@inheritDoc}
    */
    public void run()
        {
        if (m_service != null)
            {
            Cluster cluster   = CacheFactory.getCluster();
            Registry registry = cluster.getManagement();
            assert registry != null;
            
            MBeanServer server = MBeanHelper.findMBeanServer();
            try
                {
                Set set = server.queryMBeans(new ObjectName("Coherence:*"), null);
                for (Iterator iter = set.iterator(); iter.hasNext();)
                    {
                    ObjectInstance instance   = (ObjectInstance) iter.next();
                    ObjectName     objectName = instance.getObjectName();
                    if (objectName.toString().indexOf("type=Connection,") > 0)
                        {
                        setValue(server.getAttribute(objectName, "Member").toString());
                        break;
                        }
                    }
                }
            catch (Exception e)
                {
                throw Base.ensureRuntimeException(e);
                }
            }
        }

    /**
    * {@inheritDoc}
    */
    public Object getResult()
        {
        return m_sValue;
        }
        

    // ----- PortableObject interface ---------------------------------------

    /**
    * {@inheritDoc}
    */
    public void readExternal(PofReader in)
            throws IOException
        {
        m_sValue = in.readString(0);
        }

    /**
    * {@inheritDoc}
    */
    public void writeExternal(PofWriter out)
            throws IOException
        {
        out.writeString(0, m_sValue);
        }


    // ----- accessors ------------------------------------------------------

    /**
    * Set the string value.
    *
    * @param sValue  the value of the attribute
    */
    public void setValue(String sValue)
        {
        m_sValue = sValue;
        }


    // ----- data members ---------------------------------------------------

    /**
    * The string value of the attribute.
    */
    private String m_sValue;

    /**
    * The InvocationService that is executing this Invocable.
    */
    private transient InvocationService m_service;
    }
