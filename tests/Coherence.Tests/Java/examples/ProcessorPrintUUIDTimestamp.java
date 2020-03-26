/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;
import com.tangosol.io.pof.PortableObject;

import com.tangosol.util.InvocableMap.Entry;
import com.tangosol.util.UUID;

import com.tangosol.util.processor.AbstractProcessor;

import java.io.IOException;

import java.text.SimpleDateFormat;

import java.util.Date;
import java.util.TimeZone;

public class ProcessorPrintUUIDTimestamp
        extends AbstractProcessor
        implements PortableObject
    {
    // ----- constructors ---------------------------------------------------

    public ProcessorPrintUUIDTimestamp()
        {
        }

    // ----- InvocableMap.EntryProcessor interface --------------------------

    public Object process(Entry entry)
        {
        // format to .NET DateTime output form: "1/10/2013 12:19:40 PM", UTC
        SimpleDateFormat simpleDateFormat = new SimpleDateFormat("M/d/yyyy h:mm:ss a");
        simpleDateFormat.setTimeZone(TimeZone.getTimeZone("UTC"));
        return simpleDateFormat.format(new Date(((UUID) entry.getValue()).getTimestamp()));
        }

    // ----- PortableObject interface ---------------------------------------

    public void readExternal(PofReader reader) throws IOException
        {
        }

    public void writeExternal(PofWriter writer) throws IOException
        {
        }
    }
