/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */

package security;


import com.tangosol.net.CacheService;
import com.tangosol.net.ConfigurableCacheFactory;
import com.tangosol.net.CacheFactory;

import com.tangosol.net.cache.WrapperNamedCache;

import com.tangosol.util.Filter;
import com.tangosol.util.MapListener;
import com.tangosol.util.ValueExtractor;

import java.security.AccessController;
import java.security.AccessControlContext;
import java.security.DomainCombiner;
import java.security.Principal;

import java.util.Collection;
import java.util.Comparator;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import java.util.Set;

import javax.security.auth.Subject;
import javax.security.auth.SubjectDomainCombiner;


/**
* Example WrapperNamedCache that demonstrates how entitlements can be applied
* to a wrapped NamedCache using the Subject passed from the client via
* Coherence*Extend. This implementation only allows clients with a specified
* Principal name to access the wrapped NamedCache.
*
* @author jh  2006.12.15
*/
public class EntitledNamedCache
        extends WrapperNamedCache
    {
    /**
    * Create a new EntitledNamedCache.
    *
    * @param sConfig      the path to the cache configuration file used to
    *                     create wrapped caches; the EntitledNamedCache will
    *                     delegate to a cache obtained using this
    *                     configuration after first checking that the caller
    *                     is allowed access to the cache
    * @param sName        the name of the EntitledNamedCache
    * @param loader       the ClassLoader used to obtain the wrapped cache
    * @param sPrincipal1  the name of the first Principal that is allowed to
    *                     access the wrapped NamedCache
    * @param sPrincipal2  the name of the second Principal that is allowed to
    *                     access the wrapped NamedCache
    */
    public EntitledNamedCache(String sConfig, String sName, ClassLoader loader,
                              String sPrincipal1, String sPrincipal2)
        {
        super(ensureCacheFactory(sConfig).ensureCache("dist-extend-direct", loader), sName);

        if (sPrincipal1 == null || sPrincipal1.length() == 0 ||
            sPrincipal2 == null || sPrincipal2.length() == 0)
            {
            throw new IllegalArgumentException("Principal required");
            }
        m_asPrincipal = new String[]{sPrincipal1, sPrincipal2};
        }


    // ----- NamedCache interface -------------------------------------------

    /**
    * {@inheritDoc}
    */
    public String getCacheName()
        {
        checkAccess();
        return super.getCacheName();
        }

    /**
    * {@inheritDoc}
    */
    public CacheService getCacheService()
        {
        checkAccess();
        return super.getCacheService();
        }

    /**
    * {@inheritDoc}
    */
    public boolean isActive()
        {
        // must always allow access to this method, as it's called during
        // exception handling; if an initial invocation on this cache was
        // denied, we don't want a second access denial to mask the first
        // checkAccess();
        return super.isActive();
        }

    /**
    * {@inheritDoc}
    */
    public void release()
        {
        checkAccess();
        super.release();
        }

    /**
    * {@inheritDoc}
    */
    public void destroy()
        {
        checkAccess();
        super.destroy();
        }

    /**
    * {@inheritDoc}
    */
    public Object put(Object oKey, Object oValue, long cMillis)
        {
        checkAccess();
        return super.put(oKey, oValue, cMillis);
        }

    /**
    * {@inheritDoc}
    */
    public void addMapListener(MapListener listener)
        {
        checkAccess();
        super.addMapListener(listener);
        }

    /**
    * {@inheritDoc}
    */
    public void removeMapListener(MapListener listener)
        {
        checkAccess();
        super.removeMapListener(listener);
        }

    /**
    * {@inheritDoc}
    */
    public void addMapListener(MapListener listener, Object oKey, boolean fLite)
        {
        checkAccess();
        super.addMapListener(listener, oKey, fLite);
        }

    /**
    * {@inheritDoc}
    */
    public void removeMapListener(MapListener listener, Object oKey)
        {
        checkAccess();
        super.removeMapListener(listener, oKey);
        }

    /**
    * {@inheritDoc}
    */
    public void addMapListener(MapListener listener, Filter filter, boolean fLite)
        {
        checkAccess();
        super.addMapListener(listener, filter, fLite);
        }

    /**
    * {@inheritDoc}
    */
    public void removeMapListener(MapListener listener, Filter filter)
        {
        checkAccess();
        super.removeMapListener(listener, filter);
        }

    /**
    * {@inheritDoc}
    */
    public int size()
        {
        checkAccess();
        return super.size();
        }

    /**
    * {@inheritDoc}
    */
    public void clear()
        {
        checkAccess();
        super.clear();
        }

    /**
    * {@inheritDoc}
    */
    public boolean isEmpty()
        {
        checkAccess();
        return super.isEmpty();
        }

    /**
    * {@inheritDoc}
    */
    public boolean containsKey(Object oKey)
        {
        checkAccess();
        return super.containsKey(oKey);
        }

    /**
    * {@inheritDoc}
    */
    public boolean containsValue(Object oValue)
        {
        checkAccess();
        return super.containsValue(oValue);
        }

    /**
    * {@inheritDoc}
    */
    public Collection values()
        {
        checkAccess();
        return super.values();
        }

    /**
    * {@inheritDoc}
    */
    public void putAll(Map map)
        {
        checkAccess();
        super.putAll(map);
        }

    /**
    * {@inheritDoc}
    */
    public Set entrySet()
        {
        checkAccess();
        return super.entrySet();
        }

    /**
    * {@inheritDoc}
    */
    public Set keySet()
        {
        checkAccess();
        return super.keySet();
        }

    /**
    * {@inheritDoc}
    */
    public Object get(Object oKey)
        {
        checkAccess();
        return super.get(oKey);
        }

    /**
    * {@inheritDoc}
    */
    public Object remove(Object oKey)
        {
        checkAccess();
        return super.remove(oKey);
        }

    /**
    * {@inheritDoc}
    */
    public Object put(Object oKey, Object oValue)
        {
        checkAccess();
        return super.put(oKey, oValue);
        }

    /**
    * {@inheritDoc}
    */
    public Map getAll(Collection colKeys)
        {
        checkAccess();
        return super.getAll(colKeys);
        }

    /**
    * {@inheritDoc}
    */
    public boolean lock(Object oKey, long cWait)
        {
        checkAccess();
        return super.lock(oKey, cWait);
        }

    /**
    * {@inheritDoc}
    */
    public boolean lock(Object oKey)
        {
        checkAccess();
        return super.lock(oKey);
        }

    /**
    * {@inheritDoc}
    */
    public boolean unlock(Object oKey)
        {
        checkAccess();
        return super.unlock(oKey);
        }

    /**
    * {@inheritDoc}
    */
    public Set keySet(Filter filter)
        {
        checkAccess();
        return super.keySet(filter);
        }

    /**
    * {@inheritDoc}
    */
    public Set entrySet(Filter filter)
        {
        checkAccess();
        return super.entrySet(filter);
        }

    /**
    * {@inheritDoc}
    */
    public Set entrySet(Filter filter, Comparator comparator)
        {
        checkAccess();
        return super.entrySet(filter, comparator);
        }

    /**
    * {@inheritDoc}
    */
    public void addIndex(ValueExtractor extractor, boolean fOrdered, Comparator comparator)
        {
        checkAccess();
        super.addIndex(extractor, fOrdered, comparator);
        }

    /**
    * {@inheritDoc}
    */
    public void removeIndex(ValueExtractor extractor)
        {
        checkAccess();
        super.removeIndex(extractor);
        }

    /**
    * {@inheritDoc}
    */
    public Object invoke(Object oKey, EntryProcessor agent)
        {
        checkAccess();
        return super.invoke(oKey, agent);
        }

    /**
    * {@inheritDoc}
    */
    public Map invokeAll(Collection collKeys, EntryProcessor agent)
        {
        checkAccess();
        return super.invokeAll(collKeys, agent);
        }

    /**
    * {@inheritDoc}
    */
    public Map invokeAll(Filter filter, EntryProcessor agent)
        {
        checkAccess();
        return super.invokeAll(filter, agent);
        }

    /**
    * {@inheritDoc}
    */
    public Object aggregate(Collection collKeys, EntryAggregator agent)
        {
        checkAccess();
        return super.aggregate(collKeys, agent);
        }

    /**
    * {@inheritDoc}
    */
    public Object aggregate(Filter filter, EntryAggregator agent)
        {
        checkAccess();
        return super.aggregate(filter, agent);
        }


    // ----- helper methods -------------------------------------------------

    /**
    * Return a ConfigurableCacheFactory configured using the specified
    * cache configuration file.
    *
    * @param sConfig  the path to the cache configuration file
    *
    * @return a ConfigurableCacheFactory instance
    */
    protected static ConfigurableCacheFactory ensureCacheFactory(String sConfig)
        {
        if (sConfig == null || sConfig.length() == 0)
            {
            throw new IllegalArgumentException("Cache configuration file required");
            }

        Map map = m_sMapFactory;
        synchronized (map)
            {
            ConfigurableCacheFactory factory =
                    (ConfigurableCacheFactory) map.get(sConfig);
            if (factory == null)
                {
                factory = CacheFactory.getCacheFactoryBuilder().getConfigurableCacheFactory(
                    sConfig, Thread.currentThread().getContextClassLoader());
                map.put(sConfig, factory);
                }
            return factory;
            }
        }

    /**
    * Return the Subject associated with the calling thread.
    *
    * @return the current Subject or null if a Subject is not associated
    *         with the calling thread
    */
    protected Subject getCurrentSubject()
        {
        SecurityManager manager = System.getSecurityManager();
        Object oContext = manager == null
                ? AccessController.getContext()
                : manager.getSecurityContext();

        if (oContext instanceof AccessControlContext)
            {
            DomainCombiner dc = ((AccessControlContext) oContext).getDomainCombiner();
            if (dc instanceof SubjectDomainCombiner)
                {
                return ((SubjectDomainCombiner) dc).getSubject();
                }
            }

        return null;
        }

    /**
    * Assert that a Subject is associated with the calling thread with a
    * Principal with name equal to {@link #getPrincipalName()}.
    *
    * @throws SecurityException if a Subject is not associated with the
    *         calling thread or does not have the specified Principal
    */
    protected void checkAccess()
        {
        Subject subject = getCurrentSubject();
        if (subject == null)
            {
            throw new SecurityException("Access denied, authentication required");
            }

        for (Iterator iter = subject.getPrincipals().iterator(); iter.hasNext();)
            {
            Principal principal = (Principal) iter.next();
            if (m_asPrincipal[0].equals(principal.getName()) ||
                m_asPrincipal[1].equals(principal.getName()))
                {
                return;
                }
            }

        throw new SecurityException("Access denied, insufficient privileges");
        }


    // ----- accessors ------------------------------------------------------

    /**
    * Return the name of the Principal that is allowed to access the wrapped
    * cache.
    *
    * @return the name of the Principal
    */
    public String[] getPrincipalNames()
        {
        return m_asPrincipal;
        }


    // ----- data members ---------------------------------------------------

    /**
    * A static map of ConfigurableCacheFactory instances, keyed by the
    * name of the configuration file used to create the instance.
    */
    private static Map m_sMapFactory = new HashMap();

    /**
    * The names of the principal that is allowed to access the wrapped cache.
    */
    private String[] m_asPrincipal;
    }