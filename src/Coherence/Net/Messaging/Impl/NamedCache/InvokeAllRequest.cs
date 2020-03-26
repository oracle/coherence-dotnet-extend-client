/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The InvokeRequest is a <see cref="KeySetRequest"/> sent to process
    /// one or more entries (specified by their keys) in a remote NamedCache.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.04</author>
    /// <seealso cref="IEntryProcessor"/>
    /// <seealso cref="KeySetRequest"/>
    public class InvokeAllRequest : KeySetRequest, IPriorityTask
    {
        #region Properties

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

        /// <summary>
        /// The <see cref="IEntryProcessor"/> used to process entries in a
        /// remote cache.
        /// </summary>
        /// <value>
        /// <b>IEntryProcessor</b> object.
        /// </value>
        public virtual IEntryProcessor Processor
        {
            get { return m_processor; }
            set { m_processor = value; }
        }

        #endregion
        #region IPriorityTask implementation
        /// <summary>
        /// This task's scheduling priority.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryProcessor"/> implements
        /// <see cref="IPriorityTask"/> then the value of the processor's 
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
                return Processor is IPriorityTask
                    ? ((IPriorityTask)Processor).SchedulingPriority : PriorityTaskScheduling.Standard;
            }
        }

        /// <summary>
        /// The maximum amount of time this task is allowed to run before the
        /// corresponding service will attempt to stop it.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryProcessor"/> implements
        /// <see cref="IPriorityTask"/> then the value of the processor's 
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
                return Processor is IPriorityTask
                    ? ((IPriorityTask)Processor).ExecutionTimeoutMillis : (long)PriorityTaskTimeout.Default;
            }
        }

        /// <summary>
        /// The maximum amount of time a calling thread is willing to wait
        /// for a result of the request execution.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryProcessor"/> implements
        /// <see cref="IPriorityTask"/> then the value of the processor's 
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
                return Processor is IPriorityTask
                    ? ((IPriorityTask)Processor).RequestTimeoutMillis : (long)PriorityTaskTimeout.Default;
            }
        }
        /// <summary>
        /// This method will be called if and only if all attempts to
        /// interrupt this task were unsuccesful in stopping the execution or
        /// if the execution was canceled <b>before</b> it had a chance to
        /// run at all.
        /// </summary>
        /// <remarks>
        /// If the underlying <see cref="IEntryProcessor"/> implements
        /// <see cref="IPriorityTask"/> then the processor's 
        /// RunCanceled method is called.
        /// </remarks>
        /// <param name="isAbandoned">
        /// <b>true</b> if the task has timed-out, but all attempts to
        /// interrupt it were unsuccesful in stopping the execution;
        /// otherwise the task was never started.
        /// </param>
        public void RunCanceled(bool isAbandoned)
        {
            if (Processor is IPriorityTask)
            {
                ((IPriorityTask)Processor).RunCanceled(isAbandoned);
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

            Processor = (IEntryProcessor) reader.ReadObject(2);
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

            writer.WriteObject(2, Processor);
        }

        #endregion
         
        #region Extend Overrides
        
        /// <inheritdoc />
        protected override string GetDescription()
        {
            return base.GetDescription() + ", Processor=" + Processor;
        }
        
        #endregion

        #region Data members

        /// <summary>
        /// The EntryProcessor.
        /// </summary>
        private IEntryProcessor m_processor;

        /// <summary>
        /// The type identifier for this Message class.
        /// </summary>
        public const int TYPE_ID = 54;

        #endregion
    }
}