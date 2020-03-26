/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The AggregateFilterRequest is a <see cref="FilterRequest"/> sent to
    /// aggregate one or more entries (specified by a Filter) in a remote
    /// NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="FilterRequest"/>
    /// <seealso cref="IEntryAggregator"/>
    /// <seealso cref="IFilter"/>
    public class AggregateFilterRequest : FilterRequest, IPriorityTask
    {
        #region Properties

        /// <summary>
        /// The EntryAggregator.
        /// </summary>
        public virtual IEntryAggregator Aggregator
        {
            get { return m_aggregator; }
            set { m_aggregator = value; }
        }

        /// <summary>
        /// Return the identifier for <b>Message</b> object's class.
        /// </summary>
        /// <value>
        /// An identifier that uniquely identifies <b>Message</b> object's
        /// class.
        /// </value>
        /// <seealso cref="Message.TypeId"/>
        public override int TypeId
        {
            get { return TYPE_ID; }
        }

        #endregion

        #region IPriorityTask implementation
        /// <summary>
        /// This task's scheduling priority.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryAggregator"/> implements
        /// <see cref="IPriorityTask"/> then the value of the aggregator's 
        /// SchedulingPriority attribute is returned otherwise the value
        /// of <see cref="PriorityTaskScheduling.Standard"/> is returned.
        /// </remarks>
        /// <value>
        /// One of the <see cref="PriorityTaskScheduling"/> constants.
        /// </value>
        public PriorityTaskScheduling SchedulingPriority
        {
            get
            {
                return Aggregator is IPriorityTask
                    ? ((IPriorityTask)Aggregator).SchedulingPriority : PriorityTaskScheduling.Standard;
            }
        }

        /// <summary>
        /// The maximum amount of time this task is allowed to run before the
        /// corresponding service will attempt to stop it.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryAggregator"/> implements
        /// <see cref="IPriorityTask"/> then the value of the aggregator's 
        /// ExecutionTimeoutMillis attribute is returned otherwise the value
        /// of <see cref="PriorityTaskTimeout.Default"/> is returned.
        /// </remarks>
        /// <value>
        /// The execution timeout value in millisecods or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        public long ExecutionTimeoutMillis
        {
            get
            {
                return Aggregator is IPriorityTask
                    ? ((IPriorityTask)Aggregator).ExecutionTimeoutMillis : (long)PriorityTaskTimeout.Default;
            }
        }

        /// <summary>
        /// The maximum amount of time a calling thread is willing to wait
        /// for a result of the request execution.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryAggregator"/> implements
        /// <see cref="IPriorityTask"/> then the value of the aggregator's 
        /// RequestTimeoutMillis attribute is returned otherwise the value
        /// of <see cref="PriorityTaskTimeout.Default"/> is returned.
        /// </remarks>
        /// <value>
        /// The execution timeout value in millisecods or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        public long RequestTimeoutMillis
        {
            get
            {
                return Aggregator is IPriorityTask
                    ? ((IPriorityTask)Aggregator).RequestTimeoutMillis : (long)PriorityTaskTimeout.Default;
            }
        }

        /// <summary>
        /// This method will be called if and only if all attempts to
        /// interrupt this task were unsuccesful in stopping the execution or
        /// if the execution was canceled <b>before</b> it had a chance to
        /// run at all.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryAggregator"/> implements
        /// <see cref="IPriorityTask"/> then the aggregator's 
        /// RunCanceled method is called.
        /// </remarks>
        /// <param name="isAbandoned">
        /// <b>true</b> if the task has timed-out, but all attempts to
        /// interrupt it were unsuccesful in stopping the execution;
        /// otherwise the task was never started.
        /// </param>
        public void RunCanceled(bool isAbandoned)
        {
            if (Aggregator is IPriorityTask)
            {
                ((IPriorityTask)Aggregator).RunCanceled(isAbandoned);
            }
        }
        #endregion
        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <seealso cref="Request.ReadExternal"/>
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            Aggregator = (IEntryAggregator) reader.ReadObject(2);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        /// <seealso cref="Request.WriteExternal"/>
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteObject(2, Aggregator);
        }

        #endregion
        
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", Aggregator=" + Aggregator;
        }
        
        #endregion

        #region Data members

        private IEntryAggregator m_aggregator;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 52;

        #endregion
    }
}