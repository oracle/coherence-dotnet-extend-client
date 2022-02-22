/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

using Tangosol.Util;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// An abstract base for processors that implement bundling strategy.
    /// </summary>
    /// <remarks>
    /// Assume that we receive a continuous and concurrent stream of individual
    /// operations on multiple threads in parallel. Let's also assume those individual
    /// operations have relatively high latency (network or database-related) and
    /// there are functionally analogous [bulk] operations that take a collection of
    /// arguments instead of a single one without causing the latency to grow
    /// linearly, as a function of the collection size. Examples of operations and
    /// topologies that satisfy these assumptions are:
    /// <list type="bullet">
    ///   <item> Get() and GetAll() methods for the <see cref="Tangosol.Net.INamedCache"/>
    ///       API for the partitioned cache service topology;
    ///   </item>
    ///   <item> Insert() and InsertAll() methods for the <see cref="Tangosol.Net.INamedCache"/>
    ///       API for the partitioned cache service topology;
    ///   </item>
    ///   <item> Remove() method for the <see cref="Tangosol.Net.INamedCache"/>
    ///       API for the partitioned cache service topology;
    ///   </item>
    /// </list>
    /// <p/>
    /// Under these assumptions, it's quite clear that the bundler could achieve a
    /// better utilization of system resources and better throughput if slightly
    /// delays the individual execution requests with a purpose of "bundling" them
    /// together and passing into a corresponding bulk operation. Additionally,
    /// the "bundled" request should be triggered if a bundle reaches a "preferred
    /// bundle size" threshold, eliminating a need to wait till a bundle timeout is
    /// reached.
    /// <p/>
    /// <note> 
    /// We assume that all bundle-able operations are idempotent and could be
    /// repeated if un-bundling is necessary due to a bundled operation failure.
    /// </note>
    /// </remarks>
    /// <author>gg  2007.01.28</author>
    /// <author>lh  2012.05.18</author>
    /// <since>Coherence 12.1.2</since>
    public abstract class AbstractBundler
    {
        #region Constructors

        /// <summary>
        /// Construct the bundler. By default, the timeout delay value is set to
        /// one millisecond and the auto-adjustment feature is turned on.
        /// </summary>
        public AbstractBundler()
        {
            Bundle bundle = InstantiateBundle();
            bundle.IsMaster = true;
            m_listBundle.Add(bundle);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The bundle size threshold value.
        /// </summary>
        /// <value> 
        /// The bundle size threshold, a positive value expressed in the same 
        /// units as the value returned by the <see cref="Bundle.BundleSize"/> property.
        ///</value>
        public int SizeThreshold
        {
            get { return (int) m_sizeThreshold; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Negative bundle size threshold");
                }
                m_sizeThreshold = value;

                // reset the previous value used for auto adjustment
                m_previousSizeThreshold = 0.0;
            }
        }

        /// <summary>
        /// The minimum number of threads that will trigger the bundler to
        /// switch from a pass through to a bundled mode.
        /// </summary>
        /// <value>
        /// The number of threads threshold.
        /// </value>
        public int ThreadThreshold
        {
            get { return m_threadThreshold; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Invalid thread threshold");
                }
                m_threadThreshold = value;
            }
        }

        /// <summary>
        /// The timeout delay value.
        /// </summary>
        /// <value>
        /// The timeout delay value in milliseconds. Default value is one 
        /// millisecond.
        /// </value>
        public long DelayMillis
        {
            get { return m_delayMillis; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Invalid delay value");
                }
                m_delayMillis = value;
            }
        }

        /// <summary>
        /// Specifies whether or not auto-adjustment is on. Default value is "true".
        /// </summary>
        [DefaultValue(true)]
        public bool AllowAutoAdjust { get; set; }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </summary>
        /// <value>
        /// An object that can be used to synchronize access to the
        /// <b>ICollection</b>.
        /// </value>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region statistics

        /// <summary>
        /// Adjust this Bundler's parameters according to the available 
        /// statistical information.
        /// </summary>
        public void Adjust()
        {
            Statistics stats = m_stats;

            double sizePrev = m_previousSizeThreshold;
            double sizeCurr = m_sizeThreshold;

            int thruPrev = stats.AverageThroughput;
            UpdateStatistics();
            int thruCurr = stats.AverageThroughput;

            // out("Size= " + (float) sizePrev + ", Thru=" + thruPrev + " -> "
            //  + "Size= " + (float) sizeCurr + ", Thru=" + nThruCurr);

            if (AllowAutoAdjust)
            {
                double delta = 0.0;

                if (sizePrev == 0.0)
                {
                    // the very first adjustment after reset
                    delta = Math.Max(1, 0.1 * sizeCurr);
                }
                else if (Math.Abs(thruCurr - thruPrev) <=
                            Math.Max(1, (thruCurr + thruPrev) / 100))
                {
                    // not more than 2% throughput change;
                    // with a probability of 10% lets nudge the size up to 5%
                    // in a random direction
                    int random = NumberUtils.GetRandom().Next(100);
                    if (random < 10 || Math.Abs(sizePrev - sizeCurr) < 0.001)
                    {
                        delta = Math.Max(1, 0.05 * sizeCurr);
                        if (random < 5)
                        {
                            delta = -delta;
                        }
                    }
                }
                else if (thruCurr > thruPrev)
                {
                    // the throughput has improved; keep moving the size threshold
                    // in the same direction at the same rate
                    delta = (sizeCurr - sizePrev);
                }
                else
                {
                    // the throughput has dropped; reverse the direction with half
                    // of the previous rate
                    delta = (sizePrev - sizeCurr) / 2;
                }

                if (delta != 0.0)
                {
                    double sizeNew = sizeCurr + delta;
                    if (sizeNew > 1.0)
                    {
                        // out("Adjusting size by: " +
                        //     (float) dDelta + " to " + (float) sizeNew);
                        m_previousSizeThreshold = sizeCurr;
                        m_sizeThreshold         = sizeNew;
                    }
                }
            }
        }

        /// <summary>
        /// Reset this Bundler statistics.
        /// </summary>
        public void ResetStatistics()
        {
            IList listBundle = m_listBundle;
            while (true)
            {
                try
                {
                    for (int i = 0, c = listBundle.Count; i < c; i++)
                    {
                        Bundle bundle = (Bundle) listBundle[i];

                        bundle.ResetStatistics();
                    }
                    break;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // there is theoretical possibility that the memory model causes
                    // the list size to be not in sync with the actual list storage;
                    // try again
                }
            }
            m_stats.Reset();
            m_previousSizeThreshold = 0.0;
        }

        /// <summary>
        /// Update the statistics for this Bundle.
        /// </summary>
        public void UpdateStatistics()
        {
            IList      listBundle = m_listBundle;
            Statistics stats      = m_stats;
            while (true)
            {
                try
                {
                    long totalBundles = 0L;
                    long totalSize    = 0L;
                    long totalBursts  = 0L;
                    long totalWait    = 0L;

                    for (int i = 0, c = listBundle.Count; i < c; i++)
                    {
                        Bundle bundle = (Bundle) listBundle[i];

                        totalBundles += bundle.BundleSize;
                        totalSize    += bundle.TotalSize;
                        totalBursts  += bundle.TotalBurstDuration;
                        totalWait    += bundle.TotalWaitDuration;
                    }

                    long deltaBundles = totalBundles - stats.BundleCountSnapshot;
                    long deltaSize    = totalSize    - stats.BundleSizeSnapshot;
                    long deltaBurst   = totalBursts  - stats.BurstDurationSnapshot;
                    long deltaWait    = totalWait    - stats.ThreadWaitSnapshot;

                    // log("DeltaBundles=" + cDeltaBundles + ", DeltaSize=" + cDeltaSize
                    // + ", DeltaBurst=" + cDeltaBurst + ", DeltaWait=" + cDeltaWait);

                    if (deltaBundles > 0 && deltaWait > 0)
                    {
                        stats.AverageBundleSize = (int)Math.Round(
                                ((double) deltaSize) / ((double) deltaBundles));
                        stats.AverageBurstDuration = (int)Math.Round(
                                ((double) deltaBurst) / ((double) deltaBundles));
                        stats.AverageThreadWaitDuration = (int)Math.Round(
                                ((double) deltaWait) / ((double) deltaBundles));
                        stats.AverageThroughput = (int)Math.Round(
                                ((double) deltaSize * 1000) / (deltaWait));
                    }

                    stats.BundleCountSnapshot   = totalBundles;
                    stats.BundleSizeSnapshot    = totalSize;
                    stats.BurstDurationSnapshot = totalBursts;
                    stats.ThreadWaitSnapshot    = totalWait;

                    return;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // there is theoretical possibility that certain Memory Model
                    // causes the list size to be not in sync with the actual list
                    // storage; try again
                }
            }
        }

        #endregion

        /// <summary>
        /// Provide a human readable description for the Bundler object
        /// (for debugging).
        /// </summary>
        /// <returns>
        /// A human readable description for the Bundler object.
        /// </returns>
        public new String ToString()
        {
            return GetType().Name
                 + "{SizeThreshold="    + SizeThreshold
                 + ", ThreadThreshold=" + ThreadThreshold
                 + ", DelayMillis="     + DelayMillis
                 + ", AutoAdjust="      + (AllowAutoAdjust ? "on" : "off")
                 + ", ActiveBundles="   + m_listBundle.Count
                 + ", Statistics="      + m_stats
                 + "}";
        }

        #region sublcassing support

        /// <summary>
        /// Retrieve any Bundle that is currently in the open state. This method does
        /// not assume any external synchronization and as a result, a caller must
        /// double check the returned bundle open state (after synchronizing on it).
        /// </summary>
        /// <returns>
        /// An open Bundle.
        /// </returns>
        protected Bundle getOpenBundle()
        {
            IList listBundle   = m_listBundle;
            int   cBundles     = listBundle.Count;
            int   activeBundle = m_activeBundle;
            try
            {
                for (int i = 0; i < cBundles; i++)
                {
                    int    iBundle = (activeBundle + i) % cBundles;
                    Bundle bundle  = (Bundle) listBundle[iBundle];

                    if (bundle.IsOpen())
                    {
                        m_activeBundle = iBundle;
                        return bundle;
                    }
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // there is theoretical possibility that the memory model causes
                // the list size to be not in sync with the actual list storage;
                // proceed with synchronization...
            }
            catch (NullReferenceException)
            {
                // ditto
            }

            // we may need to create a new Bundle; synchronize to prevent the
            // creation of unnecessary bundles
            using (BlockingLock l = BlockingLock.Lock(listBundle))
            {
                // double check under synchronization
                cBundles = listBundle.Count;
                for (int i = 0; i < cBundles; i++)
                {
                    int    iBundle = (activeBundle + i) % cBundles;
                    Bundle bundle  = (Bundle) listBundle[iBundle];

                    if (bundle.IsOpen())
                    {
                        m_activeBundle = iBundle;
                        return bundle;
                    }
                }

                // nothing available; add a new one
                Bundle newBundle = InstantiateBundle();
                listBundle.Add(newBundle);
                m_activeBundle = cBundles;
                return newBundle;
            }
        }

        /// <summary>
        /// Instantiate a new Bundle object.
        /// </summary>
        /// <returns>
        /// A new Bundle object.
        /// </returns>
        protected abstract Bundle InstantiateBundle();

        #endregion

        #region Inner class: Bundle

        /// <summary>
        /// Bundle represents a unit of optimized execution.
        /// </summary>
        protected abstract class Bundle
        {
            #region Constructors

            /// <summary>
            /// Default constructor.
            /// </summary>
            protected Bundle()
            {
                m_status = STATUS_OPEN;
            }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="bundler">
            /// The Bundler the bundle operates on.
            /// </param>
            protected Bundle(AbstractBundler bundler)
            {
                m_status = STATUS_OPEN;
                Bundler = bundler;
            }

            #endregion

            #region Properties

            /// <summary>
            /// The bundler the operations are performed on.
            /// </summary>
            public AbstractBundler Bundler { get; set; }

            /// <summary>
            /// Gets an object that can be used to synchronize calls to a
            /// method.
            /// </summary>
            /// <value>
            /// An object that can be used to synchronize calls to a
            /// method.
            /// </value>
            public virtual object SyncRoot
            {
                get { return this; }
            }

            // stat fields intentionally have the "package private" access to
            // prevent generation of synthetic access methods

            /// <summary>
            /// Bundle size. The return value should be expressed in the
            /// same units as the value returned by the
            /// <see cref="AbstractBundler.SizeThreshold"/> property.
            /// </summary>
            public int BundleSize
            {
                get { return m_cThreads; }
            }

            /// <summary>
            /// A flag that differentiates the "master" bundle which is
            /// responsible for all auto-adjustments. It's set to "true" for
            /// one and only one Bundle object.
            /// </summary>
            public bool IsMaster { get; set; }

            /// <summary>
            /// Statistics: the total number of times this Bundle has been
            /// used for bundled request processing.
            /// </summary>
            public long TotalBundles { get; set; }

            /// <summary>
            /// Statistics: the total size of individual requests processed
            /// by this Bundle expressed in the same units as values returned
            /// by the <see cref="Bundle.BundleSize"/> method.
            /// </summary>
            public long TotalSize { get; set; }

            /// <summary>
            /// Statistics: a total time duration this Bundle has spent in
            /// bundled request processing (burst).
            /// </summary>
            public long TotalBurstDuration { get; set; }

            /// <summary>
            /// Statistics: a total time duration this Bundle has spent
            /// waiting for bundle to be ready for processing.
            /// </summary>
            public long TotalWaitDuration { get; set; }

            /// <summary>
            /// An object that serves as a mutex for thread synchronization.
            /// </summary>
            /// <remarks>
            /// When idle, the bundle thread is waiting for a notification on
            /// the Lock object.
            /// </remarks>
            /// <value>
            /// An object that serves as a mutex for thread synchronization.
            /// </value>
            /// <seealso cref="Monitor.PulseAll(object)"/>
            /// <seealso cref="Blocking.Wait(object)"/>
            protected virtual object Lock
            {
                get
                {
                    object lockObject = m_lock;
                    return lockObject == null ? this : lockObject;
                }
                set { m_lock = value; }
            }

            #endregion

            /// <summary>
            /// Check whether or not this bundle is open for adding request elements.
            /// </summary>
            /// <returns>
            /// True iff this Bundle is still open.
            /// </returns>
            public bool IsOpen()
            {
                return m_status == STATUS_OPEN;
            }

            /// <summary>
            /// Check whether or not this bundle is in the "pending" state - awaiting
            /// for the execution results.
            /// </summary>
            /// <returns>
            /// True iff this Bundle is in the "pending" state.
            /// </returns>
            protected bool IsPending()
            {
                return m_status == STATUS_PENDING;
            }

            /// <summary>
            /// Check whether or not this bundle is in the "processed" state -
            /// ready to return the result of execution back to the client.
            /// </summary>
            /// <returns>
            /// True iff this Bundle is in the "processed" state.
            /// </returns>
            protected bool IsProcessed()
            {
                return m_status == STATUS_PROCESSED ||
                       m_status == STATUS_EXCEPTION;
            }

            /// <summary>
            /// Check whether or not this bundle is in the "exception" state -
            /// bundled execution threw an exception and requests have to be
            /// un-bundled.
            /// </summary>
            /// <returns>
            /// True iff this Bundle is in the "exception" state.
            /// </returns>
            protected bool IsException()
            {
                return m_status == STATUS_EXCEPTION;
            }

            /// <summary>
            /// Change the status of this Bundle.
            /// </summary>
            /// <param name="status">
            /// The new status value.
            /// </param>
            protected void SetStatus(int status)
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    bool isValid;
                    switch (m_status)
                    {
                        case STATUS_OPEN:
                            isValid = status == STATUS_PENDING
                                || status == STATUS_EXCEPTION;
                            break;

                        case STATUS_PENDING:
                            isValid = status == STATUS_PROCESSED
                                || status == STATUS_EXCEPTION;
                            break;

                        case STATUS_PROCESSED:
                        case STATUS_EXCEPTION:
                            isValid = status == STATUS_OPEN;
                            break;
                        default:
                            isValid = false;
                            break;
                    }

                    if (!isValid)
                    {
                        throw new InvalidOperationException(this +
                            "; invalid transition to " +
                            FormatStatusName(status));
                    }

                    m_status = status;

                    if (status == STATUS_PROCESSED
                        || status == STATUS_EXCEPTION)
                    {
                        TotalWaitDuration +=
                            Math.Max(0L, DateTimeUtils.GetSafeTimeMillis() - m_startTime);
                        Monitor.PulseAll(Lock);
                    }
                }
            }

            // ----- processing and subclassing support --------------------------

            /// <summary>
            /// Obtain results of the bundled requests. This method should be
            /// implemented by concrete Bundle implementations using the most
            /// efficient mechanism.
            /// </summary>
            public abstract void EnsureResults();

            /// <summary>
            /// Wait until results of bundled requests are retrieved.
            /// </summary>
            /// <note>
            /// Calls to this method must be externally synchronized.
            /// </note>
            /// <param name="isFirst">
            /// True iff this is the first thread entering the bundle
            /// </param>
            /// <returns>
            /// True if this thread is supposed to perform an actual bundled
            /// operation (burst); false otherwise
            /// </returns>
            public bool WaitForResults(bool isFirst)
            {
                m_cThreads++;
                try
                {
                    if (isFirst)
                    {
                        m_startTime = DateTimeUtils.GetSafeTimeMillis();
                    }

                    if (BundleSize < Bundler.SizeThreshold)
                    {
                        if (isFirst)
                        {
                            long lDelay = Bundler.DelayMillis;
                            do
                            {
                                Blocking.Wait(Lock, (int) lDelay);
                                lDelay = 0L;
                            } 
                            while (IsPending());
                        }
                        else
                        {
                            while (true)
                            {
                                Blocking.Wait(Lock);

                                if (IsProcessed())
                                {
                                    return false;
                                }
                                // spurious wake-up; continue waiting
                            }
                        }
                    }

                    if (IsProcessed())
                    {
                        return false;
                    }

                    // this bundle should be closed and processed right away
                    SetStatus(STATUS_PENDING);

                    // update stats
                    TotalSize += BundleSize;
                    long total = ++TotalBundles;
                    if (total > 1000 // allow the "hotspot" to kick in
                        && total % ADJUSTMENT_FREQUENCY == 0 && IsMaster)
                    {
                        // attempt to adjust for every 1000 iterations of the master
                        // bundle
                        Bundler.Adjust();
                    }
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                    SetStatus(STATUS_EXCEPTION);
                }
                catch (Exception e)
                {
                    // should never happen
                    --m_cThreads;
                    throw e;
                }
                return true;
            }

            /// <summary>
            /// Obtain results of the bundled requests or ensure that the results
            /// have already been retrieved.
            /// </summary>
            /// <param name="isBurst">
            /// Specifies whether or not the actual results have to be
            /// fetched on this thread; this parameter will be true
            /// for one and only one thread per bundle
            /// </param>
            /// <returns>
            /// True if the bundling has succeeded; false if the un-bundling
            /// has to be performed as a result of a failure.
            /// </returns>
            protected bool EnsureResults(bool isBurst)
            {
                if (IsException())
                {
                    return false;
                }

                if (isBurst)
                {
                    // bundle is closed and ready for the actual execution (burst);
                    // it must be performed without holding any synchronization
                    try
                    {
                        long startTime = DateTimeUtils.GetSafeTimeMillis();

                        EnsureResults();

                        long elapsedMillis = DateTimeUtils.GetSafeTimeMillis() - startTime;
                        if (elapsedMillis > 0)
                        {
                            TotalBurstDuration += elapsedMillis;
                        }

                        SetStatus(STATUS_PROCESSED);
                    }
                    catch (Exception)
                    {
                        SetStatus(STATUS_EXCEPTION);
                        return false;
                    }
                }
                else
                {
                    Debug.Assert(IsProcessed());
                }
                return true;
            }

            /// <summary>
            /// Release all bundle resources associated with the current thread.
            /// </summary>
            /// <returns>
            /// True iff all entered threads have released.
            /// </returns>
            protected bool ReleaseThread()
            {
                using (BlockingLock l = BlockingLock.Lock(SyncRoot))
                {
                    Debug.Assert(IsProcessed() && m_cThreads > 0);

                    if (--m_cThreads == 0)
                    {
                        SetStatus(STATUS_OPEN);
                        return true;
                    }
                }
                return false;
            }

            #region statistics and debugging

            /// <summary>
            /// Reset statistics for this Bundle.
            /// </summary>
            public void ResetStatistics()
            {
                TotalBundles       = 0L;
                TotalSize          = 0L;
                TotalBurstDuration = 0L;
                TotalWaitDuration  = 0L;
            }

            /// <summary>
            /// Provide a human readable description for the Bundle object
            /// (for debugging).
            /// </summary>
            /// <returns>
            /// A human readable description for the Bundle object.
            /// </returns>
            public new String ToString()
            {
                return "Bundle@" + GetHashCode() + "{" + FormatStatusName(m_status)
                     + ", size=" + BundleSize + '}';
            }

            /// <summary>
            /// Return a human readable name for the specified status value.
            /// </summary>
            /// <param name="status">
            /// The status value to format.
            /// </param>
            /// <returns>
            /// A human readable status name.
            /// </returns>
            protected static String FormatStatusName(int status)
            {
                switch (status)
                {
                    case STATUS_OPEN:
                        return "STATUS_OPEN";
                    case STATUS_PENDING:
                        return "STATUS_PENDING";
                    case STATUS_PROCESSED:
                        return "STATUS_PROCESSED";
                    case STATUS_EXCEPTION:
                        return "STATUS_EXCEPTION";
                    default:
                        return "unknown";
                }
            }

            #endregion

            #region Data Members and Constants

            /// <summary>
            /// This Bundle accepting additional items.
            /// </summary>
            public const int STATUS_OPEN = 0;

            /// <summary>
            /// This Bundle is closed for accepting additional items and awaiting
            /// for the execution results.
            /// </summary>
            public const int STATUS_PENDING = 1;

            /// <summary>
            /// This Bundle is in process of returning the result of execution
            /// back to the client.
            /// </summary>
            public const int STATUS_PROCESSED = 2;

            /// <summary>
            /// Attempt to bundle encountered and exception; the execution has to be
            /// de-optimized and performed by individual threads.
            /// </summary>
            public const int STATUS_EXCEPTION = 3;

            /// <summary>
            /// This Bundle status.
            /// </summary>
            private volatile int m_status = STATUS_OPEN;

            /// <summary>
            /// A count of threads that are using this Bundle.
            /// </summary>
            private int m_cThreads;

            /// <summary>
            /// Statistics: a timestamp of the first thread entering the bundle.
            /// </summary>
            private long m_startTime;

            /// <summary>
            /// An object that serves as a mutex for this Daemon synchronization.
            /// </summary>
            [NonSerialized] 
            private object m_lock;

            #endregion
        }

        #endregion

        #region Inner class: Statistics
        /// <summary>
        /// Statistics class contains the latest bundler statistics.
        /// </summary>
        public class Statistics
        {
            #region Properties

            // ----- running averages ------------------------------------------

            /// <summary>
            /// An average bundle size for this Bundler.
            /// </summary>
            public int AverageBundleSize { get; set; }

            /// <summary>
            /// An average time for bundled request processing (burst).
            /// </summary>
            public int AverageBurstDuration { get; set; }

            /// <summary>
            /// An average thread waiting time caused by under-filled bundle. The
            /// wait time includes the time spend in the bundled request processing.
            /// </summary>
            public int AverageThreadWaitDuration { get; set; }

            /// <summary>
            /// An average bundled request throughput in size units per millisecond
            /// (total bundle size over total processing time)
            /// </summary>
            public int AverageThroughput { get; set; }

            // ----- snapshots --------------------------------------------------

            /// <summary>
            /// Snapshot for a total number of processed bundled.
            /// </summary>
            public long BundleCountSnapshot { get; set; }

            /// <summary>
            /// Snapshot for a total size of processed bundled.
            /// </summary>
            public long BundleSizeSnapshot { get; set; }

            /// <summary>
            /// Snapshot for a burst duration.
            /// </summary>
            public long BurstDurationSnapshot { get; set; }

            /// <summary>
            /// Snapshot for a combined thread waiting time.
            /// </summary>
            public long ThreadWaitSnapshot { get; set; }

            #endregion

            /// <summary>
            /// Reset the statistics.
            /// </summary>
            public void Reset()
            {
                BundleCountSnapshot   = 0L;
                BundleSizeSnapshot    = 0L;
                BurstDurationSnapshot = 0L;
                ThreadWaitSnapshot    = 0L;
            }

            /// <summary>
            /// Provide a human readable description for the Statistics object.
            /// (for debugging).
            /// </summary>
            /// <returns>
            /// A human readable description for the Statistics object.
            /// </returns>
            public new String ToString()
            {
                return "(AverageBundleSize="     + AverageBundleSize
                     + ", AverageBurstDuration=" + AverageBurstDuration      + "ms"
                     + ", AverageWaitDuration="  + AverageThreadWaitDuration + "ms"
                     + ", AverageThroughput="    + AverageThroughput         + "/sec"
                     + ")";
            }
        }

        #endregion

        #region Data members and Constants

        /// <summary>
        /// Frequency of the adjustment attempts. This number represents a number of
        /// iterations of the master bundle usage after which an adjustment attempt
        /// will be performed.
        /// </summary>
        public const int ADJUSTMENT_FREQUENCY = 128;

        /// <summary>
        /// The bundle size threshold. We use double for this value to allow for
        /// fine-tuning of the auto-adjust algorithm.
        /// </summary>
        /// <see cref="AbstractBundler.Adjust"/>
        private double m_sizeThreshold;

        /// <summary>
        /// The previous bundle size threshold value.
        /// </summary>
        protected double m_previousSizeThreshold;

        /// <summary>
        /// The minimum number of threads that should trigger the bundler to switch
        /// from a pass through mode to a bundled mode.
        /// </summary>
        private int m_threadThreshold;

        /// <summary>
        /// The delay timeout in milliseconds. Default value is one millisecond.
        /// </summary>
        private long m_delayMillis = 1L;

        /// <summary>
        /// A pool of Bundle objects. Note that this list never shrinks.
        /// </summary>
        protected IList m_listBundle = new ArrayList();

        /// <summary>
        /// Last active (open) bundle position.
        /// </summary>
        private volatile int m_activeBundle;

        /// <summary>
        /// A counter for the total number of threads that have started any bundle
        /// related execution. This counter is used by subclasses to reduce an impact
        /// of bundled execution for lightly loaded environments.
        /// </summary>
        protected AtomicCounter m_countThreads = AtomicCounter.NewAtomicCounter();


        /// <summary>
        /// An instance of the Statistics object containing the latest statistics.
        /// </summary>
        private readonly Statistics m_stats = new Statistics();

        #endregion
    }
}