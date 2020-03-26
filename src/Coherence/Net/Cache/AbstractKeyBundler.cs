/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Util;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An abstract key-based bundler serves as a base for NamedCache get() and
    /// remove() operation bundling.
    /// </summary>
    /// <author>gg  2007.01.28</author>
    /// <author>lh  2012.05.22</author>
    /// <since>Coherence 12.1.2</since>
    public abstract class AbstractKeyBundler : AbstractBundler
    {
        #region bundling support

        /// <summary>
        /// Process the specified key in a most optimal way according to the
        /// bundle settings.
        /// </summary>
        /// <param name="key">
        /// The key to process.
        /// </param>
        /// <returns> 
        /// An execution result according to the caller's contract.
        /// </returns>
        public Object Process(Object key)
        {
            AtomicCounter counter  = m_countThreads;
            int           cThreads = (int) counter.Increment();
            try
            {
                if (cThreads < ThreadThreshold)
                {
                    return Unbundling(key);
                }

                Bundle bundle;
                bool   isBurst;
                while (true)
                {
                    bundle = (Bundle)getOpenBundle();
                    lock (bundle)
                    {
                        if (bundle.IsOpen())
                        {
                            bool isFirst = bundle.Add(key);

                            isBurst = bundle.WaitForResults(isFirst);
                            break;
                        }
                    }
                }
                return bundle.Process(isBurst, key);
            }
            finally
            {
                counter.Decrement();
            }
        }

        /// <summary>
        /// Process a colKeys of specified items in a most optimal way according to
        /// the bundle settings.
        /// </summary>
        /// <param name="colKeys">
        /// The collection of keys to process.
        /// </param>
        /// <returns>
        /// An execution result according to the caller's contract.
        /// </returns>
        public IDictionary ProcessAll(ICollection colKeys)
        {
            AtomicCounter counter  = m_countThreads;
            int           cThreads = (int) counter.Increment();
            try
            {
                if (cThreads < ThreadThreshold)
                {
                    return Bundling(colKeys);
                }

                Bundle bundle;
                bool   isBurst;
                while (true)
                {
                    bundle = (Bundle) getOpenBundle();
                    lock (bundle)
                    {
                        if (bundle.IsOpen())
                        {
                            bool isFirst = bundle.AddAll(colKeys);

                            isBurst = bundle.WaitForResults(isFirst);
                            break;
                        }
                    }
                }
                return bundle.ProcessAll(isBurst, colKeys);
            }
            finally
            {
                counter.Decrement();
            }
        }

        #endregion

        #region subclassing support

        /// <summary>
        /// The bundle operation to be performed against a collected set of keys
        /// by the concrete AbstractKeyBundler implementations. If an exception
        /// occurs during bundle operation, it could be repeated using singleton sets.
        /// </summary>
        /// <param name="colKeys">
        /// A key collection to perform the bundled operation for.
        /// </param>
        /// <returns>
        /// The Map of operation results.
        /// </returns>
        public abstract IDictionary Bundling(ICollection colKeys);

        /// <summary>
        /// Un-bundle bundled operation. This operation would be used if an 
        /// exception occurs during a bundled operation or if the number of 
        /// active threads is below the <see cref="AbstractBundler.ThreadThreshold"/>
        /// value.
        /// </summary>
        /// <param name="key">
        /// A key to perform the un-bundled operation for.
        /// </param>
        /// <returns>
        /// The operation result for the specified key, may be null.
        /// </returns>
        public abstract Object Unbundling(Object key);

        /// <summary>
        /// Instantiate a new Bundle object.
        /// </summary>
        /// <returns>
        /// A new Bundle object.
        /// </returns>
        protected override AbstractBundler.Bundle InstantiateBundle()
        {
            return new Bundle(this);
        }

        #endregion

        #region Inner class: Bundle

        /// <summary>
        /// Bundle represents a unit of optimized execution.
        /// </summary>
        protected new class Bundle : AbstractBundler.Bundle
        {
            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            public Bundle()
            {}

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="bundler">
            /// The bundler the operations are performed on.
            /// </param>
            public Bundle(AbstractBundler bundler)
                : base(bundler)
            {}

            #endregion

            #region bundling support

            /// <summary>
            /// Add the specified key to the Bundle.
            /// </summary>
            /// <note>
            /// A call to this method must be externally synchronized
            /// for this Bundle object.
            /// </note>
            /// <param name="key">
            /// The key to add to this Bundle.
            /// </param>
            /// <returns>
            /// True if this Bundle was empty prior to this call.
            /// </returns>
            public bool Add(Object key)
            {
                ICollection setKeys = m_setKeys;
                bool        isFirst = setKeys.Count == 0;
                CollectionUtils.Add(setKeys, key);
                return isFirst;
            }

            /// <summary>
            /// Add the specified collection of keys to the Bundle.
            /// </summary>
            /// <note>
            /// A call to this method must be externally synchronized for
            /// this Bundle object.
            /// </note>
            /// <param name="colKeys">
            /// The collection of keys to add to this Bundle.
            /// </param> 
            /// <returns>
            /// True if this Bundle was empty prior to this call.
            /// </returns>
            public bool AddAll(ICollection colKeys)
            {
                ICollection setKeys = m_setKeys;
                bool        isFirst = setKeys.Count == 0;
                CollectionUtils.AddAll(setKeys, colKeys);
                return isFirst;
            }

            /// <summary>
            /// Process the specified key according to this Bundle state.
            /// </summary>
            /// <param name="isBurst">
            /// True if this thread is supposed to perform an actual bundled 
            /// operation (burst); false otherwise.
            /// </param>
            /// <param name="key">
            /// The key to process.
            /// </param>
            /// <returns>
            /// An execution result according to the caller's contract.
            /// </returns>
            public Object Process(bool isBurst, Object key)
            {
                try
                {
                    return EnsureResults(isBurst)
                        ? m_mapResults[key] : ((AbstractKeyBundler)Bundler).Unbundling(key);
                }
                finally
                {
                    ReleaseThread();
                }
            }

            /// <summary>
            /// Process the specified key collection according to this Bundle state.
            /// </summary>
            /// <param name="isBurst">
            /// True if this thread is supposed to perform an actual bundled
            /// operation (burst); false otherwise.
            /// </param>
            /// <param name="colKeys">
            /// The collection of keys to process.
            /// </param>
            /// <returns>
            /// An execution result according to the caller's contract.
            /// </returns>
            public IDictionary ProcessAll(bool isBurst, ICollection colKeys)
            {
                try
                {
                    if (EnsureResults(isBurst))
                    {
                        IDictionary mapResults = m_mapResults;
                        IDictionary map        = new Hashtable(colKeys.Count);
                        foreach (Object key in colKeys)
                        {
                            Object value = mapResults[key];
                            if (value != null)
                            {
                                map.Add(key, value);
                            }
                        }
                        return map;
                    }
                    else
                    {
                        return ((AbstractKeyBundler) Bundler).Bundling(colKeys);
                    }
                }
                finally
                {
                    ReleaseThread();
                }
            }

            /// <summary>
            /// Bundle size. The return value should be expressed in the
            /// same units as the value returned by the
            /// <see cref="AbstractBundler.SizeThreshold"/> property.
            /// </summary>
            /// <returns>
            /// Bundle size.
            /// </returns>
            protected int GetBundleSize()
            {
                return Math.Max(BundleSize, m_setKeys.Count);
            }

            /// <summary>
            /// Obtain results of the bundled requests.
            /// </summary>
            public override void EnsureResults()
            {
                m_mapResults = ((AbstractKeyBundler) Bundler).Bundling(m_setKeys);
            }

            /// <summary>
            /// Release all bundle resources associated with the current thread.
            /// </summary>
            /// <returns>
            /// True iff all entered threads have released
            /// </returns>
            public new bool ReleaseThread()
            {
                lock (SyncRoot)
                {
                    bool isReleased = base.ReleaseThread();
                    if (isReleased)
                    {
                        // TODO: unfortunately, clear() will drop the underlying bucket
                        // array, which may cause unnecessary resizing later on...
                        // ideally, we would want to preserve the bucket count, but
                        // clear the content; consider adding SafeHashSet.clearContent()
                        m_setKeys.Clear();
                        m_mapResults = null;
                    }
                    return isReleased;
                }
            }

            #endregion

            #region Data Members

            /// <summary>
            /// This bundle content.
            /// </summary>
            private readonly SafeHashSet m_setKeys = new SafeHashSet();

            /// <summary>
            /// A result of the bundled processing.
            /// </summary>
            private IDictionary m_mapResults;

            #endregion
        }

        #endregion
    }
}