/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;

using Tangosol.Util.Collections;

namespace Tangosol.Net.Internal
{
    /// <summary>
    /// ScopedReferenceStore holds scoped cache or service references.
    /// </summary>
    /// <remarks>
    /// Cache references are scoped by name and, optionally, by Principal.
    /// Service references are scoped by name and, optionally, by Principal.
    /// Principal scoping is handled automatically; ScopedReferenceStore
    /// requires no explicit input about Principals from its clients. Principal
    /// scoping is configured in the operational configuration and applies only
    /// to remote caches and remote services.
    /// <p>
    /// An instance of ScopedReferenceStore must contain either cache
    /// references or service references, but not both simultaneously.
    /// </p>
    /// <p>
    /// ScopedReferenceStore is not thread-safe unless a lock is obtained on
    /// the SyncRoot property; otherwise, multi-threaded clients must
    /// provide their own locking mechanism.
    /// </p>
    /// </remarks>
    /// <author>David Guy  2010.03.17</author>
    /// <since>Coherence 3.6</since>
    public class ScopedReferenceStore
    {
        #region ScopedReferenceStore implementation

        /// <summary>
        /// Remove all referenced objects from this store.
        /// </summary>
        public virtual void Clear()
        {
            m_mapByName.Clear();
        }

        /// <summary>
        /// Retrieve the cache reference associated with the name (and
        /// Principal if applicable).
        /// </summary>
        /// <param name="sCacheName">
        /// The name of the cache.
        /// </param>
        /// <returns>
        /// The cache reference.
        /// </returns>
        public virtual INamedCache GetCache(string sCacheName)
        {
            object oHolder = m_mapByName[sCacheName];

            if (oHolder == null)
            {
                return null;
            }
            if (oHolder is INamedCache)
            {
                return (INamedCache) oHolder;
            }
            if (oHolder is PrincipalScopedReference)
            {
                return (INamedCache) ((PrincipalScopedReference) oHolder).Get();
            }
            throw new NotSupportedException();
        }

        /// <summary>
        /// Retrieve the Service reference based on the passed in service name.
        /// </summary>
        /// <param name="sServiceName">
        /// The service name.
        /// </param>
        /// <returns>
        /// The service reference.
        /// </returns>
        public virtual IService GetService(string sServiceName)
        {
            object oHolder = m_mapByName[sServiceName];

            if (oHolder == null)
            {
                return null;
            }
            if (oHolder is IService)
            {
                return (IService) oHolder;
            }
            if (oHolder is PrincipalScopedReference)
            {
                return (IService) ((PrincipalScopedReference) oHolder).Get();
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Retrieve all cache references in the store.
        /// </summary>
        /// <returns>
        /// All cache references.
        /// </returns>
        public virtual IEnumerable<INamedCache> GetAllCaches()
        {
            foreach (object oHolder in m_mapByName.Values)
            {
                if (oHolder is PrincipalScopedReference)
                {
                    foreach (var value in ((PrincipalScopedReference) oHolder).Caches)
                    {
                        yield return value;
                    }
                }
                else if (oHolder is INamedCache)
                {
                    yield return oHolder as INamedCache;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Retreive all cache references for this name.
        /// </summary>
        /// <param name="sCacheName">
        /// The name of the cache.
        /// </param>
        /// <returns>
        /// All cache references for this name.
        /// </returns>
        public virtual IEnumerable<INamedCache> GetAllCaches(string sCacheName)
        {
            object oHolder = m_mapByName[sCacheName];

            if (oHolder is PrincipalScopedReference)
            {
                foreach (var value in ((PrincipalScopedReference) oHolder).Caches)
                {
                    yield return value;
                }
            }
            else if (oHolder is INamedCache)
            {
                yield return oHolder as INamedCache;
                yield break;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Retrieve all service references in the store.
        /// </summary>
        /// <returns>
        /// All service references.
        /// </returns>
        public virtual IEnumerable<IService> GetAllServices()
        {
            foreach (object oHolder in m_mapByName.Values)
            {
                if (oHolder is PrincipalScopedReference)
                {
                    foreach (var value in ((PrincipalScopedReference) oHolder).Services)
                    {
                        yield return value;
                    }
                }
                else if (oHolder is IService)
                {
                    yield return oHolder as IService;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Retrieve the names of all stored cache or service references.
        /// </summary>
        /// <returns>
        /// The names of all stored references.
        /// </returns>
        public virtual IEnumerable<string> GetNames()
        {
            foreach (string name in m_mapByName.Keys)
            {
                yield return name;
            }
        }

        /// <summary>
        /// Store a cache reference.
        /// </summary>
        /// <param name="cache">
        /// The cache reference.
        /// </param>
        public virtual void PutCache(INamedCache cache)
        {
            IDictionary   mapByName    = m_mapByName;
            string        sCacheName   = cache.CacheName;
            ICacheService cacheService = cache.CacheService;

            if (cacheService != null && IsRemoteServiceType(
                cacheService.Info.ServiceType) && OperationalContext.IsPrincipalScopingEnabled)
            {
                PrincipalScopedReference scopedRef =
                    (PrincipalScopedReference) mapByName[sCacheName];
                if (scopedRef == null)
                {
                    scopedRef = new PrincipalScopedReference();
                    mapByName[sCacheName] = scopedRef;
                }
                scopedRef.Set(cache);
            }
            else
            {
                mapByName[sCacheName] = cache;
            }
        }

        /// <summary>
        /// Store a service reference.
        /// </summary>
        /// <remarks>
        /// Service name and type are passed in rather than using
        /// service.Info because the service may not have been configured
        /// and started yet, so the Info may not be safely available.
        /// </remarks>
        /// <param name="service">
        /// The referenced service.
        /// </param>
        /// <param name="sName">
        /// The service name.
        /// </param>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        public virtual void PutService(IService service, string sName,
            ServiceType serviceType)
        {
            IDictionary mapByName = m_mapByName;

            if (IsRemoteServiceType(serviceType) && OperationalContext.IsPrincipalScopingEnabled)
            {
                PrincipalScopedReference scopedRef =
                    (PrincipalScopedReference) mapByName[sName];
                if (scopedRef == null)
                {
                    scopedRef = new PrincipalScopedReference();
                    mapByName[sName] = scopedRef;
                }
                scopedRef.Set(service);
            }
            else
            {
                mapByName[sName] = service;
            }
        }

        /// <summary>
        /// Remove the cache reference from the store.
        /// </summary>
        /// <param name="cache">
        /// The cache reference.
        /// </param>
        /// <returns>
        /// Whether the item was found.
        /// </returns>
        public virtual bool ReleaseCache(INamedCache cache)
        {
            IDictionary mapByName  = m_mapByName;
            string      sCacheName = cache.CacheName;
            bool        fFound     = false;

            object oHolder = mapByName[sCacheName];

            if (oHolder == cache)
            {
                // remove the mapping
                mapByName.Remove(sCacheName);
                fFound = true;
            }
            else if (oHolder is PrincipalScopedReference)
            {
                PrincipalScopedReference scopedRef = (PrincipalScopedReference)
                                                     oHolder;

                if (scopedRef.Get() == cache)
                {
                    scopedRef.Remove();
                    fFound = true;
                }
            }
            return fFound;
        }

        /// <summary>
        /// Remove items referenced by this name.
        /// </summary>
        /// <param name="sName">
        /// The service or cache name.
        /// </param>
        public virtual void Remove(string sName)
        {
            m_mapByName.Remove(sName);
        }

        /// <summary>
        /// The <see cref="IOperationalContext"/> for this ScopedReferenceStore.
        /// </summary>
        /// <value>
        /// An <b>IOperationalContext</b> instance.
        /// </value>
        public virtual IOperationalContext OperationalContext
        {
            get { return m_operationalContext; }
        }

        #endregion

        #region constructor

        ///<summary>
        /// constructor takes IOperationalContext
        ///</summary>
        ///<param name="operationalContext">
        /// The OperationalContext for ScopedReferenceStore.
        /// </param>
        public ScopedReferenceStore(IOperationalContext operationalContext)
        {
            m_operationalContext = operationalContext;
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Determine if the service type is remote.
        /// </summary>
        /// <param name="serviceType">
        /// The service type.
        /// </param>
        /// <returns>
        /// Whether the service type is remote.
        /// </returns>
        protected static bool IsRemoteServiceType(ServiceType serviceType)
        {
            return serviceType == ServiceType.RemoteCache ||
                   serviceType == ServiceType.RemoteInvocation;
        }

        #endregion

    #region Inner class: PrincipalScopedReference

        /// <summary>
        /// PrincipalScopedReference scopes (associates) an object with a
        /// Principal.
        /// </summary>
        public class PrincipalScopedReference
        {
            #region Properties

            /// <summary>
            /// Obtain all referenced objects.
            /// </summary>
            /// <value>
            /// The enumeration of referenced caches.
            /// </value>
            public virtual IEnumerable<INamedCache> Caches
            {
                get
                {
                    object oRef = m_oRef;

                    if (oRef != null)
                    {
                        yield return oRef as INamedCache;
                    }
                    foreach (INamedCache value in m_mapPrincipalScope.Values)
                    {
                        yield return value;
                    }
                }
            }

            /// <summary>
            /// Obtain all referenced objects.
            /// </summary>
            /// <value>
            /// The enumeration of referenced services.
            /// </value>
            public virtual IEnumerable<IService> Services
            {
                get
                {
                    object oRef = m_oRef;

                    if (oRef != null)
                    {
                        yield return oRef as IService;
                    }
                    foreach (IService value in m_mapPrincipalScope.Values)
                    {
                        yield return value;
                    }
                }
            }

            #endregion

            #region PrincipalScopedReference implementation
            /// <summary>
            /// Obtain the object referenced by the current principal.
            /// </summary>
            /// <returns>
            /// The referenced object.
            /// </returns>
            public virtual object Get()
            {
                IPrincipal principal = Thread.CurrentPrincipal;

                return principal == null ? m_oRef : m_mapPrincipalScope[principal];
            }

            /// <summary>
            /// Determine if there are any referenced objects.
            /// </summary>
            /// <returns>
            /// Whether there are any referenced objects.
            /// </returns>
            public virtual bool IsEmpty()
            {
                IPrincipal principal = Thread.CurrentPrincipal;

                return principal == null ? (m_oRef == null)
                        : m_mapPrincipalScope.Count > 0;
            }

            /// <summary>
            /// Add a referenced object based on the current principal.
            /// </summary>
            /// <param name="oRef">
            /// The referenced object.
            /// </param>
            public virtual void Set(object oRef)
            {
                IPrincipal principal = Thread.CurrentPrincipal;

                if (principal == null)
                {
                    m_oRef = oRef;
                }
                else
                {
                    m_mapPrincipalScope[principal] = oRef;
                }
            }

            /// <summary>
            /// Remove the object referenced by the current principal.
            /// </summary>
            /// <returns>
            /// The previously referenced object.
            /// </returns>
            public virtual object Remove()
            {
                IPrincipal  principal = Thread.CurrentPrincipal;
                IDictionary mapScope  = m_mapPrincipalScope;
                object      oPrev;

                if (principal == null)
                {
                    oPrev = m_oRef;
                    m_oRef = null;
                }
                else
                {
                    oPrev = mapScope[principal];
                    mapScope.Remove(principal);
                }
                return oPrev;
            }

            #endregion

            #region Data members

            /// <summary>
            /// The Dictionary contains referenced objects keyed by Principal.
            /// </summary>
            private readonly IDictionary m_mapPrincipalScope = new HashDictionary();

            /// <summary>
            /// Reference stored when the Principal is null.
            /// </summary>
            private object m_oRef;

            #endregion
        }

        #endregion

        #region Data members
        /// <summary>
        /// When storing cache references, it is a Dictionary keyed by a cache
        /// name with a corresponding value being an IINamedCache reference or
        /// a PrincipalScopedReference that contains a IINamedCache reference.
        /// <p>
        /// When storing service references, it is a Dictionary keyed by a service
        /// name with a corrsponding value being a ICacheService reference or a
        /// PrincipalScopedReference that contains an ICacheService reference.
        /// </p>
        /// </summary>
        protected readonly IDictionary  m_mapByName = new SynchronizedDictionary();

        /// <summary>
        /// The IOperationalContext for this ScopedReferenceStore.
        /// </summary>
        protected readonly IOperationalContext m_operationalContext;

        #endregion


    }
}