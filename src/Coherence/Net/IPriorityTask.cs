/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Messaging;

namespace Tangosol.Net
{
    /// <summary>
    /// The IPriorityTask interface allows to control the ordering in which a
    /// service schedules tasks for execution using a thread pool and limit
    /// their execution times to a specified duration.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Instances of IPriorityTask typically also implement either
    /// <see cref="IInvocable"/> or <see cref="IRunnable"/> interface.</p>
    /// <p>
    /// Depending on the value of <see cref="SchedulingPriority"/> property,
    /// the scheduling order will be one of the following:
    /// <list type="bullet">
    /// <item>
    /// <term><see cref="PriorityTaskScheduling.Standard"/></term>
    /// <description>
    /// a task will be scheduled for execution in a natural (based on the
    /// request arrival time) order;
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="PriorityTaskScheduling.First"/></term>
    /// <description>
    /// a task will be scheduled in front of any equal or lower scheduling
    /// priority tasks and executed as soon as any of worker threads become
    /// available;
    /// </description>
    /// </item>
    /// <item>
    /// <term><see cref="PriorityTaskScheduling.Immediate"/></term>
    /// <description>
    /// a task will be immediately executed by any idle worker thread; if all
    /// of them are active, a new thread will be created to execute this
    /// task.
    /// </description>
    /// </item>
    /// </list></p>
    /// <p>
    /// A best effort will be made to limit the task execution time according
    /// to the value of the <see cref="ExecutionTimeoutMillis"/> property.
    /// However,it should be noted that:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// for tasks with the scheduling priority of
    /// <see cref="PriorityTaskScheduling.Immediate"/>, factors that could
    /// make the execution time longer than the timeout value are long GC
    /// pauses and high network latency;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// if the service has a task backlog (when there are more tasks
    /// scheduled for execution than the number of available worker threads),
    /// the request execution time (measured from the client's perspective)
    /// for tasks with the scheduling priorities of
    /// <see cref="PriorityTaskScheduling.Standard"/> or
    /// <see cref="PriorityTaskScheduling.First"/> could be longer and
    /// include the time those tasks were kept in a queue before invocation;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the corresponding service is free to cancel the task execution before
    /// the task is started and call the <see cref="RunCanceled"/> method if
    /// it's known that the client is no longer interested in the results
    /// of the task execution.
    /// </description>
    /// </item>
    /// </list></p>
    /// <p>
    /// In addition to allowing control of the task execution (as scheduled
    /// and measured on the server side), the IPriorityTask interface could
    /// also be used to control the request time from the calling thread
    /// perspective (measured on the client). A best effort will be made to
    /// limit the request time (the time period that the calling thread is
    /// blocked waiting for a response from the corresponding service) to the
    /// value of the <see cref="RequestTimeoutMillis"/> property.</p>
    /// <p>
    /// It should be noted that the request timeout value (RT) could be
    /// grater than, equal to or less than the task execution timeout value
    /// (ET). The value of RT which is less than ET indicates that even
    /// though the task execution is allowed to take longer period of time,
    /// the client thread will not wait for a result of the execution and
    /// will be able to handle a timeout exception if it arises. Since the
    /// time spent by the task waiting in the service backlog queue does not
    /// count toward the task execution time, a value of RT that is equal or
    /// slightly greater than ET still leaves a possibility that the client
    /// thread will throw a TimeoutException before the task completes its
    /// execution normally on a server.</p>
    /// </remarks>
    /// <author>Gene Gleyzer  2006.11.02</author>
    /// <author>Ana Cikic  2007.05.17</author>
    /// <since>Coherence 3.3</since>
    public interface IPriorityTask
    {
        /// <summary>
        /// This task's scheduling priority.
        /// </summary>
        /// <value>
        /// One of the <see cref="PriorityTaskScheduling"/> constants.
        /// </value>
        PriorityTaskScheduling SchedulingPriority { get;}

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
        /// termination, the <see cref="RunCanceled"/> method will be called.
        /// </p>
        /// </remarks>
        /// <value>
        /// The execution timeout value in millisecods or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        long ExecutionTimeoutMillis { get; }

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
        /// <see cref="ExecutionTimeoutMillis"/> value.</p>
        /// <p>
        /// If the specified amount of time elapsed and the client has not
        /// received any response from the server, a RequestTimeoutException
        /// will be thrown to the caller.</p>
        /// </remarks>
        /// <value>
        /// The request timeout value in milliseconds or one of the special
        /// <see cref="PriorityTaskTimeout"/> values.
        /// </value>
        long RequestTimeoutMillis { get; }

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
        void RunCanceled(bool isAbandoned);
    }

    /// <summary>
    /// <see cref="IPriorityTask"/> scheduling priority constants.
    /// </summary>
    public enum PriorityTaskScheduling
    {
        /// <summary>
        /// Scheduling value indicating that this task is to be queued and
        /// executed in a natural (based on the request arrival time) order.
        /// </summary>
        Standard = 0,

        /// <summary>
        /// Scheduling value indicating that this task is to be queued in
        /// front of any equal or lower scheduling priority tasks and
        /// executed as soon as any of the worker threads become available.
        /// </summary>
        First = 1,

        /// <summary>
        /// Scheduling value indicating that this task is to be immediately
        /// executed by any idle worker thread; if all of them are active, a
        /// new thread will be created to execute this task.
        /// </summary>
        Immediate = 2
    }

    /// <summary>
    /// <see cref="IPriorityTask"/> timeout constants.
    /// </summary>
    public enum PriorityTaskTimeout
    {
        /// <summary>
        /// A special timeout value to indicate that the corresponding
        /// service's default timeout value should be used. 
        /// </summary>
        Default = 0,

        /// <summary>
        /// A special timeout value to indicate that this task or request can
        /// run indefinitely.
        /// </summary>
        None = -1
    }
}