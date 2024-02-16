/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Net.Cache;
using Tangosol.Util;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// Bundling NamedCache implementation.
    /// </summary>
    /// <see cref="AbstractBundler"/>
    /// <author>gg 2007.01.28</author>
    /// <author>lh 2012.05.18</author>
    /// <since>Coherence 12.1.2</since>
    public class BundlingNamedCache : WrapperNamedCache
    {
        #region Constructors

        /// <summary>
        /// Construct a BundlingNamedCache based on the specified <see cref="INamedCache"/>.
        /// </summary>
        /// <param name="cache">
        /// The INamedCache that will be wrapped by this BundlingNamedCache
        /// </param>
        public BundlingNamedCache(INamedCache cache)
            : base(cache)
        {
        }

        #endregion

        #region initiators

        /// <summary>
        /// Gets an object that can be used to synchronize calls to a
        /// method.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize calls to a
        /// method.
        /// </value>
        public virtual new object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Configure the bundler for the "get" operations. If the bundler does not
        /// exist and bundling is enabled, it will be instantiated.
        /// </summary>
        /// <param name="bundleThreshold">
        /// The bundle size threshold; pass zero to disable "Get" operation
        /// bundling.
        /// </param>
        /// <returns>
        /// The "Get" bundler or null if bundling is disabled.
        /// </returns>
        public AbstractBundler EnsureGetBundler(int bundleThreshold)
        {
            using (BlockingLock l = BlockingLock.Lock(SyncRoot))
            {
                if (bundleThreshold > 0)
                {
                    var bundler = (GetBundler) GetBundlerOp;
                    if (bundler == null)
                    {
                        GetBundlerOp = bundler = new GetBundler();
                        bundler.BundleNamedCache = this;
                    }
                    bundler.SizeThreshold = bundleThreshold;
                    return bundler;
                }
                return GetBundlerOp = null;
            }
        }

        /// <summary>
        /// Configure the bundler for the "Insert" operations. If the bundler
        /// does not exist and bundling is enabled, it will be instantiated.
        /// </summary>
        /// <param name="bundleThreshold">
        /// The bundle size threshold; pass zero to disable "Insert" operation.
        /// bundling
        /// </param>
        /// <returns> 
        /// The "Insert" bundler or null if bundling is disabled.
        /// </returns>
        public AbstractBundler EnsureInsertBundler(int bundleThreshold)
        {
            using (BlockingLock l = BlockingLock.Lock(SyncRoot))
            {
                if (bundleThreshold > 0)
                {
                    var bundler = (InsertBundler) InsertBundlerOp;
                    if (bundler == null)
                    {
                        InsertBundlerOp = bundler = new InsertBundler();
                        bundler.BundleNamedCache = this;
                    }
                    bundler.SizeThreshold = bundleThreshold;
                    return bundler;
                }
                return InsertBundlerOp = null;
            }
        }

        /// <summary>
        /// Configure the bundler for the "Remove" operations. If the bundler
        /// does not exist and bundling is enabled, it will be instantiated.
        /// </summary>
        /// <param name="bundleThreshold">
        /// The bundle size threshold; pass zero to disable "Remove" operation
        /// bundling.
        /// </param>
        /// <returns>
        /// The "Remove" bundler or null if bundling is disabled.
        /// </returns>
        public AbstractBundler EnsureRemoveBundler(int bundleThreshold)
        {
            using (BlockingLock l = BlockingLock.Lock(SyncRoot))
            {
                if (bundleThreshold > 0)
                {
                    var bundler = (RemoveBundler) RemoveBundlerOp;
                    if (bundler == null)
                    {
                        RemoveBundlerOp = bundler = new RemoveBundler();
                        bundler.BundleNamedCache = this;
                    }
                    bundler.SizeThreshold = bundleThreshold;
                    return bundler;
                }
                return RemoveBundlerOp = null;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The bundler for Get() operations.
        /// </summary>
        public AbstractBundler GetBundlerOp { get; set; }

        /// <summary>
        /// The bundler for Insert() operations.
        /// </summary>
        public AbstractBundler InsertBundlerOp { get; set; }

        /// <summary>
        /// The bundler for Remove() operations.
        /// </summary>
        public AbstractBundler RemoveBundlerOp { get; set; }

        #endregion

        #region various bundleable INamedCache methods

        /// <summary>
        /// Get the value for a specified key, if it is in the
        /// cache.
        /// </summary>
        /// <param name="key">
        /// The key of the element to get.
        /// </param>
        public Object Get(Object key)
        {
            var bundler = (GetBundler) GetBundlerOp;
            return bundler == null ?
                base[key] : bundler.Process(key);
        }

        /// <summary>
        /// Call the GetAll() method in the base class.
        /// </summary>
        /// <param name="colKeys">
        /// A collection of keys for the GetAll operation.
        /// </param>
        protected IDictionary BaseGetAll(ICollection colKeys)
        {
            return base.GetAll(colKeys);
        }

        /// <summary>
        /// Get the values for all the specified keys, if they are in the
        /// cache.
        /// </summary>
        /// <param name="colKeys">
        /// A collection of keys for the GetAll operation.
        /// </param>
        public new IDictionary GetAll(ICollection colKeys)
        {
            var bundler = (GetBundler) GetBundlerOp;
            return bundler == null ?
                BaseGetAll(colKeys) : bundler.ProcessAll(colKeys);
        }

        /// <summary>
        /// Associates the specified value with the specified key in this
        /// cache.
        /// </summary>
        /// <param name="key">
        /// Key with which the specified value is to be associated.
        /// </param>
        /// <param name="value">
        /// Value to be associated with the specified key.
        /// </param>
        /// <note>
        /// This method always returns null.
        /// </note>
        public override Object Insert(Object key, Object value)
        {
            var bundler = (InsertBundler) InsertBundlerOp;
            if (bundler == null)
            {
                IDictionary map = new Hashtable();
                map.Add(key, value);
                BaseInsertAll(map);
            }
            else
            {
                bundler.Process(key, value);
            }
            return null;
        }

        /// <summary>
        /// Call the InsertAll() method in the base class.
        /// </summary>
        /// <param name="dictionary">
        /// Dictionary to be stored in this cache.
        /// </param>
        protected void BaseInsertAll(IDictionary dictionary)
        {
            base.InsertAll(dictionary);
        }

        /// <summary>
        /// Copies all of the mappings from the specified dictionary to this
        /// cache (optional operation).
        /// </summary>
        /// <param name="dictionary">
        /// Dictionary to be stored in this cache.
        /// </param>
        public override void InsertAll(IDictionary dictionary)
        {
            var bundler = (InsertBundler) InsertBundlerOp;
            if (bundler == null)
            {
                BaseInsertAll(dictionary);
            }
            else
            {
                bundler.ProcessAll(dictionary);
            }
        }

        /// <summary>
        /// Call the Remove() method in the base class.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <note>
        /// This method always returns null.
        /// </note>
        protected Object BaseRemove(Object key)
        {
            base.Remove(key);
            return null;
        }

        /// <summary>
        /// Removes the element with the specified key from the <b>IDictionary</b>
        /// object.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <note>
        /// This method always returns null.
        /// </note>
        public new Object Remove(Object key)
        {
            var bundler = (RemoveBundler) RemoveBundlerOp;
            if (bundler == null)
            {
                BaseRemove(key);
            }
            else
            {
                bundler.Process(key);
            }
            return null;
        }

        #endregion

        #region inner classes

        /// <summary>
        /// The bundler for Get operations.
        /// </summary>
        public class GetBundler : AbstractKeyBundler
        {
            /// <summary>
            /// The bundling cache to perform the operation on.
            /// </summary>
            public BundlingNamedCache BundleNamedCache { get; set; }

            /// <summary>
            /// A pass through the underlying GetAll operation.
            /// </summary>
            /// <param name="colKeys">
            /// A collection of keys for the GetAll operation.
            /// </param>
            public override IDictionary Bundling(ICollection colKeys)
            {
                return BundleNamedCache.BaseGetAll(colKeys);
            }

            /// <summary>
            /// A pass through the underlying Get operation.
            /// </summary>
            /// <param name="key">
            /// The key of the element to get.
            /// </param>
            public override Object Unbundling(Object key)
            {
                return BundleNamedCache[key];
            }
        }

        /// <summary>
        /// The bundler for Insert operations.
        /// </summary>
        public class InsertBundler : AbstractEntryBundler
        {
            /// <summary>
            /// The bundling cache to perform the operation on.
            /// </summary>
            public BundlingNamedCache BundleNamedCache { get; set; }

            /// <summary>
            /// A pass through the underlying InsertAll() operation.
            /// </summary>
            /// <param name="dictionary">
            /// A dictionary to perform the bundled operation for.
            /// </param>
            public override void Bundling(IDictionary dictionary)
            {
                BundleNamedCache.BaseInsertAll(dictionary);
            }
        }

        /// <summary>
        /// The bundler for Remove operations.
        /// </summary>
        public class RemoveBundler : AbstractKeyBundler
        {
            /// <summary>
            /// The bundling cache to perform the operation on.
            /// </summary>
            public BundlingNamedCache BundleNamedCache { get; set; }

            /// <summary>
            /// A pass through the underlying Keys.RemoveAll() operation.
            /// </summary>
            /// <param name="colKeys">
            /// A collection of keys for the RemoveAll operation.
            /// </param>
            public override IDictionary Bundling(ICollection colKeys)
            {
                CollectionUtils.RemoveAll(BundleNamedCache.Keys, colKeys);
                return null;
            }

            /// <summary>
            /// A pass through the underlying Remove() operation.
            /// </summary>
            /// <param name="key">
            /// The key of the element to remove.
            /// </param>
            public override Object Unbundling(Object key)
            {
                return BundleNamedCache.BaseRemove(key);
            }
        }

        #endregion
    }
}