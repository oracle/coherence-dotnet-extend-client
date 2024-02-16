/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An abstract entry-based bundler serves as a base for 
    /// NamedCache.Insert() operation bundling.
    /// </summary>
    /// <author>gg  2007.01.28</author>
    /// <author>lh  2012.05.22</author>
    /// <since>Coherence 12.1.2</since>
    public abstract class AbstractEntryBundler : AbstractBundler
    {
        #region bundling support

        /// <summary>
        /// Process the specified entry in a most optimal way according
        /// to the bundle settings.
        /// </summary>
        /// <param name="key">
        /// The entry key.
        /// </param>
        /// <param name="value">
        /// The entry value.
        /// </param>
        public void Process(Object key, Object value)
        {
            AtomicCounter counter  = m_countThreads;
            int           cThreads = (int) counter.Increment();
            try
            {
                if (cThreads < ThreadThreshold)
                {
                    IDictionary dictionary = new Hashtable();
                    dictionary.Add(key, value);
                    Bundling(dictionary);
                    return;
                }

                Bundle bundle;
                bool   isBurst;
                while (true)
                {
                    bundle = (Bundle) getOpenBundle();
                    using (BlockingLock l = BlockingLock.Lock(bundle))
                    {
                        if (bundle.IsOpen())
                        {
                            bool isFirst = bundle.Add(key, value);

                            isBurst = bundle.WaitForResults(isFirst);
                            break;
                        }
                    }
                }
                bundle.Process(isBurst, key, value);
            }
            finally
            {
                counter.Decrement();
            }
        }

        /// <summary>
        /// Process a colllection of entries in a most optimal way 
        /// according to the bundle settings.
        /// </summary>
        /// <param name="dictionary">
        /// The collection of entries to process
        /// </param>
        public void ProcessAll(IDictionary dictionary)
        {
            AtomicCounter counter  = m_countThreads;
            var           cThreads = (int) counter.Increment();
            try
            {
                if (cThreads < ThreadThreshold)
                {
                    Bundling(dictionary);
                    return;
                }

                Bundle bundle;
                bool   isBurst;
                while (true)
                {
                    bundle = (Bundle) getOpenBundle();
                    using (BlockingLock l = BlockingLock.Lock(bundle))
                    {
                        if (bundle.IsOpen())
                        {
                            bool isFirst = bundle.AddAll(dictionary);

                            isBurst = bundle.WaitForResults(isFirst);
                            break;
                        }
                    }
                }
                bundle.ProcessAll(isBurst, dictionary);
            }
            finally
            {
                counter.Decrement();
            }
        }

        #endregion

        #region subclassing support

        /// <summary>
        /// The bundle operation to be performed against a collected 
        /// dictionary of entries by the concrete AbstractEntryBundler 
        /// implementations. If an exception occurs during bundle 
        /// operation, it will be repeated using singleton dictionaries.
        /// </summary>
        /// <param name="dictionary">
        /// A dictionary of entries to perform the bundled operation for.
        /// </param>
        abstract public void Bundling(IDictionary dictionary);

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
            protected Bundle()
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
            /// Add the specified entry to the Bundle.
            /// </summary>
            /// <note>
            /// A call to this method must be externally synchronized for
            /// this Bundle object.
            /// </note>
            /// <param name="key">
            /// The entry key.
            /// </param>
            /// <param name="value">
            /// The entry value.
            /// </param>
            /// <returns>
            /// True if this Bundle was empty prior to this call.
            /// </returns>
            public bool Add(Object key, Object value)
            {
                IDictionary dictionary = m_mapEntries;
                bool        isFirst    = dictionary.Count == 0;
                dictionary.Add(key, value);
                return isFirst;
            }

            /// <summary>
            /// Add the specified collection of entries to the Bundle.
            /// </summary>
            /// <note>
            /// A call to this method must be externally synchronized
            /// for this Bundle object.
            /// </note>
            /// <param name="dictionary">
            /// The collection of entries.
            /// </param>
            /// <returns>
            /// True if this Bundle was empty prior to this call.
            /// </returns>
            public bool AddAll(IDictionary dictionary)
            {
                IDictionary mapEntries = m_mapEntries;
                bool        isFirst    = mapEntries.Count == 0;
                CollectionUtils.AddAll(mapEntries, dictionary);
                return isFirst;
            }

            /// <summary>
            /// Process the specified entry according to this Bundle state.
            /// </summary>
            /// <param name="isBurst">
            /// True if this thread is supposed to perform an actual bundled
            /// operation (burst); false otherwise
            /// </param>
            /// <param name="key">
            /// The entry key.
            /// </param>
            /// <param name="value">
            /// The entry value.
            /// </param>
            public void Process(bool isBurst, Object key, Object value)
            {
                try
                {
                    if (!EnsureResults(isBurst))
                    {
                        IDictionary dictionary = new Hashtable();
                        dictionary.Add(key, value);
                        ((AbstractEntryBundler) Bundler).Bundling(dictionary);
                    }
                }
                finally
                {
                    ReleaseThread();
                }
            }

            /// <summary>
            /// Process the specified collection of entries according to 
            /// this Bundle state.
            /// </summary>
            /// <param name="isBurst">
            /// True if this thread is supposed to perform an actual
            /// bundled operation (burst); false otherwise
            /// </param>
            /// <param name="dictionary">
            /// The collection of entries.
            /// </param>
            public void ProcessAll(bool isBurst, IDictionary dictionary)
            {
                try
                {
                    if (!EnsureResults(isBurst))
                    {
                        ((AbstractEntryBundler) Bundler).Bundling(dictionary);
                    }
                }
                finally
                {
                    ReleaseThread();
                }
            }

            #endregion

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
                return Math.Max(BundleSize, m_mapEntries.Count);
            }

            /// <summary>
            /// Obtain results of the bundled requests.
            /// </summary>
            public override void EnsureResults()
            {
                ((AbstractEntryBundler) Bundler).Bundling(m_mapEntries);
            }

            /// <summary>
            /// Release all bundle resources associated with the current thread.
            /// </summary>
            /// <returns>
            /// True iff all entered threads have released.
            /// </returns>
            protected new bool ReleaseThread()
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    bool isReleased = base.ReleaseThread();
                    if (isReleased)
                    {
                        // TODO: unfortunately, clear() will drop the 
                        // underlying bucket array, which may cause 
                        // unnecesary resizing later on... ideally, we
                        // would want to preserve the bucket count, but
                        // clear the content; consider adding SafeHashMap.clearContent()
                        m_mapEntries.Clear();
                    }
                    return isReleased;
                }
            }

            #region Data Members

            /// <summary>
            /// This bundle content.
            /// </summary>
            private readonly IDictionary m_mapEntries = new Hashtable();

            #endregion
        }

        #endregion
    }
}