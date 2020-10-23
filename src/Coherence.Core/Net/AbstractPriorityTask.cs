/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;

namespace Tangosol.Net
{
    /// <summary>
    /// An abstract base for <see cref="IPriorityTask"/> implementations.
    /// </summary>
    /// <remarks>
    /// It implements all <b>IPriorityTask</b> interface methods and is
    /// intended to be extended for concrete uses.
    /// </remarks>
    /// <author>Gene Gleyzer  2007.03.20</author>
    /// <since>Coherence 3.3</since>
    public abstract class AbstractPriorityTask : IPriorityTask, IPortableObject
    {
        #region IPriorityTask implementation

        /// <summary>
        /// This task's scheduling priority.
        /// </summary>
        /// <value>
        /// One of the <see cref="PriorityTaskScheduling"/> constants.
        /// </value>
        public virtual PriorityTaskScheduling SchedulingPriority
        {
            get { return m_schedulingPriority; }
            set { m_schedulingPriority = value; }
        }

        /// <summary>
        /// The maximum amount of time this task is allowed to run before the
        /// corresponding service will attempt to stop it.
        /// </summary>
        /// <remarks>
        /// <p>
        /// The value of <see cref="PriorityTaskTimeout.Default"/> indicates
        /// a default timeout value configured for the corresponding service;
        /// the value of <see cref="PriorityTaskTimeout.None"/> indicates
        /// that this task can execute indefinitely.</p>
        /// <p>
        /// If, by the time the specified amount of time passed, the task has
        /// not finished, the service will attempt to stop the execution by
        /// using the <b>Thread.Interrupt()</b> method. In the case that
        /// interrupting the thread does not result in the task's
        /// termination, the <see cref="IPriorityTask.RunCanceled"/> method will be called.
        /// </p>
        /// </remarks>
        /// <value>
        /// The execution timeout value in millisecods or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        public virtual long ExecutionTimeoutMillis
        {
            get { return m_executionTimeoutMillis; }
            set
            {
                if (value < (long) PriorityTaskTimeout.None)
                {
                    throw new ArgumentException("Invalid timeout: " + value);
                }
                m_executionTimeoutMillis = value;
            }
        }

        /// <summary>
        /// The maximum amount of time a calling thread is willing to wait
        /// for a result of the request execution.
        /// </summary>
        /// <remarks>
        /// The request time is measured on the client side as the time
        /// elapsed from the moment a request is sent for execution to the
        /// corresponding server node(s) and includes:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// the time it takes to deliver the request to the executing
        /// node(s);
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// the interval between the time the task is received and placed
        /// into a service queue until the execution starts;
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// the task execution time;
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// the time it takes to deliver a result back to the client.
        /// </description>
        /// </item>
        /// </list>
        /// <p>
        /// The value of <see cref="PriorityTaskTimeout.Default"/> indicates
        /// a default timeout value configured for the corresponding service;
        /// the value of <see cref="PriorityTaskTimeout.None"/> indicates
        /// that the client thread is willing to wait indefinitely until the
        /// task execution completes or is canceled by the service due to a
        /// task execution timeout specified by the
        /// <see cref="IPriorityTask.ExecutionTimeoutMillis"/> value.</p>
        /// <p>
        /// If the specified amount of time elapsed and the client has not
        /// received any response from the server, a RequestTimeoutException
        /// will be thrown to the caller.</p>
        /// </remarks>
        /// <value>
        /// The request timeout value in milliseconds or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        public virtual long RequestTimeoutMillis
        {
            get { return m_requestTimeoutMillis; }
            set
            {
                if (value < (long) PriorityTaskTimeout.None)
                {
                    throw new ArgumentException("Invalid timeout: " + value);
                }
                m_requestTimeoutMillis = value;
            }
        }

        /// <summary>
        /// This method will be called if and only if all attempts to
        /// interrupt this task were unsuccesful in stopping the execution or
        /// if the execution was canceled <b>before</b> it had a chance to
        /// run at all.
        /// </summary>
        /// <remarks>
        /// Since this method is usually called on a service thread,
        /// implementors must exercise extreme caution since any delay
        /// introduced by the implementation will cause a delay of the
        /// corresponding service.
        /// </remarks>
        /// <param name="isAbandoned">
        /// <b>true</b> if the task has timed-out, but all attempts to
        /// interrupt it were unsuccesful in stopping the execution;
        /// otherwise the task was never started.
        /// </param>
        public virtual void RunCanceled(bool isAbandoned)
        {}

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <remarks>
        /// The AbstractPriorityTask implementation reserves property indexes
        /// 0 - 9.
        /// </remarks>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {
            m_schedulingPriority     = (PriorityTaskScheduling) reader.ReadInt32(0);
            m_executionTimeoutMillis = reader.ReadInt64(1);
            m_requestTimeoutMillis   = reader.ReadInt64(2); 
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <remarks>
        /// The AbstractPriorityTask implementation reserves property indexes
        /// 0 - 9.
        /// </remarks>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, (int) m_schedulingPriority);
            writer.WriteInt64(1, m_executionTimeoutMillis);
            writer.WriteInt64(2, m_requestTimeoutMillis);
        }

        #endregion

        #region Data members

        private PriorityTaskScheduling m_schedulingPriority     = PriorityTaskScheduling.Standard;
        private long                   m_executionTimeoutMillis = (long) PriorityTaskTimeout.Default;
        private long                   m_requestTimeoutMillis   = (long) PriorityTaskTimeout.Default;

        #endregion
    }
}