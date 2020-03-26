/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import com.tangosol.io.ExternalizableLite;
import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;
import com.tangosol.io.pof.PortableObject;
import com.tangosol.net.GuardSupport;
import com.tangosol.net.PriorityTask;
import com.tangosol.util.ExternalizableHelper;
import com.tangosol.util.InvocableMap;
import com.tangosol.util.processor.AbstractProcessor;

import java.io.DataInput;
import java.io.DataOutput;
import java.io.IOException;

/**
 * @author jk 2016.03.29
 */
public class SlowProcessor
        extends AbstractProcessor
        implements PortableObject
    {

    public SlowProcessor()
        {
        }

    public SlowProcessor(long cMillis, String sReturnValue)
        {
        m_cMillis      = cMillis;
        m_sReturnValue = sReturnValue;
        }

    @Override
    public Object process(InvocableMap.Entry entry)
        {
        System.out.println("Executing SlowProcessor: m_cMillis=" + m_cMillis);
 
        try
            {
            long cSteps = m_cMillis / 1000;

            for (long i = 0; i< cSteps; i++)
                {
                Thread.sleep(1000);
                GuardSupport.heartbeat();
                }
            }
        catch (InterruptedException e)
            {
            e.printStackTrace();
            }


        System.out.println("Finished SlowProcessor: m_sReturnValue=" + m_sReturnValue);

        return m_sReturnValue;
        }

    @Override
    public void readExternal(PofReader in) throws IOException
        {
        m_cMillis      = in.readLong(0);
        m_sReturnValue = in.readString(1);
        }

    @Override
    public void writeExternal(PofWriter out) throws IOException
        {
        out.writeLong(0, m_cMillis);
        out.writeString(1, m_sReturnValue);
        }


    private long m_cMillis;

    private String m_sReturnValue;
    }