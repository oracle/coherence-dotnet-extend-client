/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import java.io.IOException;

import com.tangosol.io.ReadBuffer.BufferInput;
import com.tangosol.io.WriteBuffer.BufferOutput;
import com.tangosol.io.pof.PofContext;
import com.tangosol.io.pof.ConfigurablePofContext;
import com.tangosol.io.Serializer;

/* Custom Serializer class to match up with the .NET
 * TestSerializer class
 */
public class CustomSerializer implements Serializer
    {
    // ----- constructors ---------------------------------------------------

    /**
    * Default constructor.
    */
    public CustomSerializer()
        {
        setPofContext(new ConfigurablePofContext());
        }

    /**
    * xml constructor.
    *
    * @param xml  configuration for serializer
    */
    public CustomSerializer(String xml)
        {
        setPofContext(new ConfigurablePofContext(xml));
        }

    // ----- Serializer interface -------------------------------------------

    public Object deserialize(BufferInput Buf) throws IOException
        {
        return getPofContext().deserialize(Buf);
        }

    public void serialize(BufferOutput Buf, Object obj) throws IOException
        {
        getPofContext().serialize(Buf, obj);
        }

    // ----- accessors ------------------------------------------------------

    protected PofContext getPofContext()
        {
        return m_Context;
        }

    protected void setPofContext(PofContext Context)
        {
        m_Context = Context;
        }

    // ----- data members ---------------------------------------------------

    protected PofContext m_Context;
    }
