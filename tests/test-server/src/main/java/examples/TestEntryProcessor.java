/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import com.tangosol.net.CacheFactory;

import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;
import com.tangosol.io.pof.PortableObject;

import com.tangosol.util.InvocableMap;
import com.tangosol.util.InvocableMap.Entry;

import com.tangosol.util.processor.AbstractProcessor;

import java.io.IOException;

public class TestEntryProcessor
        extends AbstractProcessor
        implements PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public TestEntryProcessor()
        {
        this(false);
        }

    public TestEntryProcessor(boolean fRemove)
        {
        f_fRemoveSynthetic = fRemove;
        }

    // ----- InvocableMap.EntryProcessor interface --------------------------

    @Override
    public Object process(InvocableMap.Entry entry)
        {
        CacheFactory.log("entrytype is " + entry.getClass().getName());
        if (f_fRemoveSynthetic)
            {
            entry.remove(true);
            }
        else
            {
            entry.setValue("EPSetValue", true);
            }
        return "OK";
        }

    // ----- PortableObject interface -----------------------------------

    /**
     * {@inheritDoc}
     */
    public void readExternal(PofReader in)
            throws IOException
        {
        }

    /**
     * {@inheritDoc}
     */
    public void writeExternal(PofWriter out)
            throws IOException
        {
        }

    // data members
    protected final boolean f_fRemoveSynthetic;
    }