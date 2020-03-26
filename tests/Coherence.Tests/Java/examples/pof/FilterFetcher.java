/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
/*
* FilterFetcher.java
*
* Copyright (c) 2000, 2011, Oracle and/or its affiliates. All rights reserved.
*
* Oracle is a registered trademarks of Oracle Corporation and/or its

* affiliates.
*
* This software is the confidential and proprietary information of Oracle
* Corporation. You shall not disclose such confidential and proprietary
* information and shall use it only in accordance with the terms of the
* license agreement you entered into with Oracle.
*
* This notice may not be removed or altered.
*/
package examples.pof;

import com.tangosol.io.pof.PortableObject;
import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;

import com.tangosol.net.Invocable;
import com.tangosol.net.InvocationService;

import com.tangosol.util.QueryHelper;
import com.tangosol.util.filter.InFilter;

import java.io.IOException;

import java.util.Map;

/**
* FilterFetcher supports getting Filters or ValueExtractors
* by using an InvocationService.  See FilterFactory.
*
* @author djl  2010.02.15
*/
public class FilterFetcher
        implements Invocable, PortableObject
    {
    // ----- constructors ---------------------------------------------------

    /**
    * Construct a new FilterFetcher
    */
    public FilterFetcher()
        {
        m_sQuery = "";
        }

    /**
    * Construct a new FilterFetcher that will return a Filter based on
    * the given string
    *
    * @param s  The string that defines the Filter
    **/
    public FilterFetcher(String s)
        {
        m_sQuery = s;
        }

    /**
    * Construct a new FilterFetcher that will return a Filter based on
    * the given string.  The given flag controls whether a ValueExtractor or
    * a Filter is retreived
    *
    * @param s                The string that defines the Filter
    * @param fFetchExtractor  a boolean flag that controls whether a
    *                         ValueExtractor or a Filter is retreived
    **/
    public FilterFetcher(String s, boolean fFetchExtractor)
        {
        m_sQuery          = s;
        m_fFetchExtractor = fFetchExtractor;
        }

    /**
    * Construct a new FilterFetcher that will return a Filter based on
    * the given string and binding environment.
    *
    * @param s     The string that defines the Filter
    * @param aEnv  an Object[] that specifies the binding environment
    **/
    public FilterFetcher(String s, Object[] aEnv)
        {
        m_sQuery = s;
        m_aEnv   = aEnv;
        }

    /**
    * Construct a new FilterFetcher that will return a Filter based on
    * the given string and binding environment.
    *
    * @param s         The string that defines the Filter
    * @param bindings  a Map that specifies the binding environment
    **/
    public FilterFetcher(String s, Map bindings)
        {
        m_sQuery   = s;
        m_aEnv     = null;
        m_bindings = bindings;
        }

    /**
    * Construct a new FilterFetcher that will return Filter based on
    * the given string and binding environmets.
    *
    * @param s         The string that defines the Filter
    * @param aEnv      an Object[] that specifies the binding environment
    * @param bindings  a Map that specifies the binding environment
    **/
    public FilterFetcher(String s, Object[] aEnv, Map bindings)
        {
        m_sQuery   = s;
        m_aEnv     = aEnv;
        m_bindings = bindings;
        }

    /**
    * Called by the InvocationService exactly once on this Invocable object
    * as part of its initialization.
    *
    * @param service  the containing InvocationService
    */
    public void init(InvocationService service)
        {
        m_service = service;
        }

    /**
    * {@inheritDoc}
    */
    public Object getResult()
        {
        return m_oResult;
        }

    public void run()
        {
        if (m_fFetchExtractor)
            {
            setResult(QueryHelper.createExtractor(m_sQuery));
            }
        else
            {
            setResult(QueryHelper.createFilter(m_sQuery, m_aEnv, m_bindings));
            }
        }

    /**
    * Set the result of the invocation.
    *
    * @param oResult  the invocation result
    */
    protected void setResult(Object oResult)
        {
        m_oResult = oResult;
        }

    /**
    * {@inheritDoc}
    */
    public void readExternal(PofReader reader) throws IOException
        {
        m_fFetchExtractor = reader.readBoolean(0);
        m_sQuery          = reader.readString(1);
        m_aEnv            = reader.readObjectArray(2,null);
        m_bindings        = (Map) reader.readMap(3, null);
        }

    /**
    * {@inheritDoc}
    */
    public void writeExternal(PofWriter writer) throws IOException
        {
        writer.writeBoolean    (0, m_fFetchExtractor);
        writer.writeString     (1, m_sQuery);
        writer.writeObjectArray(2, m_aEnv);
        writer.writeMap        (3, m_bindings);
        }

    // ----- data members ---------------------------------------------------

    /**
    * Flag to control whether to get ValueExtractor vs. Filter
    */
    protected boolean m_fFetchExtractor = false;

    /**
    * The query String to use
    */
    protected String m_sQuery;

    /**
    * An Array of bindings
    */
    protected Object[] m_aEnv;

    /**
    * A Map of bindings
    */
    protected Map m_bindings;

    /**
    * The InvocationService executing the request
    */
    private transient InvocationService m_service;
  
    /**
    * The query result
    */
    private Object m_oResult;
    }
