/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Net.Messaging;

namespace Tangosol.Net
{
    /// <summary>
    /// An abstract base for <see cref="IInvocable"/> and 
    /// <see cref="IPriorityTask"/> implementations.
    /// </summary>
    /// <remarks>
    /// For Invocables which only run within the Coherence cluster (most common case),
    /// the .NET Init, Result and Run methods can be left unimplemented.
    /// </remarks>
    /// <author>Gene Gleyzer  2003.03.31, 2007.03.11</author>
    /// <author>Ivan Cikic  2007.05.18</author>
    public abstract class AbstractInvocable : IInvocable, IPriorityTask
    {
        #region Properties

        /// <summary>
        /// Obtain the containing <see cref="IInvocationService"/>.
        /// </summary>
        /// <value>
        /// The containing <b>IInvocationService</b>.
        /// </value>
        public IInvocationService Service
        {
            get; private set;
        }

        #endregion
                
        #region IInvocable interface implementation

        /// <summary>
        /// Called by the <see cref="IInvocationService"/> exactly once on
        /// this <see cref="IInvocable"/> object as part of its 
        /// initialization.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> implementations of the <b>IInvocable</b> interface
        /// that store the service reference must do so only in a transient 
        /// field.
        /// </remarks>
        /// <param name="service">
        /// The containing <b>InvocationService</b>.
        /// </param>
        public virtual void Init(IInvocationService service)
        {
            Service = service;
        }

        /// <summary>
        /// Gets or sets the result of the invocation of this object.
        /// </summary>
        /// <remarks>
        /// This property value is read after the <see cref="IRunnable.Run"/>
        /// method returns.
        /// </remarks>
        /// <value>
        /// The object representing result from the invocation.
        /// </value>
        public object Result
        {
            get; set;
        }

        #endregion
        
        #region IPriorityTask interface implementation

        /// <summary>
        /// This task's scheduling priority.
        /// </summary>
        /// <remarks>
        /// This implementation returns 
        /// <see cref="PriorityTaskScheduling.Standard"/>.
        /// </remarks>
        /// <value>
        /// One of the <see cref="PriorityTaskScheduling"/> constants.
        /// </value>
        public PriorityTaskScheduling SchedulingPriority
        {
            get { return PriorityTaskScheduling.Standard; }
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
        /// termination, the <see cref="IPriorityTask.RunCanceled"/>
        /// method will be called.</p>
        /// <p>
        /// This implementation returns 
        /// <see cref="PriorityTaskTimeout.Default"/></p>
        /// </remarks>
        /// <value>
        /// The execution timeout value in millisecods or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        public long ExecutionTimeoutMillis
        {
            get { return (long) PriorityTaskTimeout.Default; }
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
        /// <p>
        /// This implementation returns 
        /// <see cref="PriorityTaskTimeout.Default"/></p>
        /// </remarks>
        /// <value>
        /// The request timeout value in milliseconds or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        public long RequestTimeoutMillis
        {
            get { return (long) PriorityTaskTimeout.Default; }
        }

        /// <summary>
        /// This method will be called if and only if all attempts to
        /// interrupt this task were unsuccesful in stopping the execution or
        /// if the execution was canceled <b>before</b> it had a chance to
        /// run at all.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Since this method is usually called on a service thread,
        /// implementors must exercise extreme caution since any delay
        /// introduced by the implementation will cause a delay of the
        /// corresponding service.</p>
        /// <p>
        /// This implementation is a no-op.</p>
        /// </remarks>
        /// <param name="isAbandoned">
        /// <b>true</b> if the task has timed-out, but all attempts to
        /// interrupt it were unsuccesful in stopping the execution;
        /// otherwise the task was never started.
        /// </param>
        public virtual void RunCanceled(bool isAbandoned)
        {
        }

        #endregion

        #region IRunnable interface

        /// <summary>
        /// Execute the action specific to the object implementation.
        /// </summary>
        /// <remarks>
        /// This implementation throws a NotSupportedException.
        /// </remarks>
        public virtual void Run()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IPortableObject interface

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
        public abstract void ReadExternal(IPofReader reader);

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
        public abstract void WriteExternal(IPofWriter writer);

        #endregion
    }
}
