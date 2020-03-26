/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.IO.Pof;
using Tangosol.Net.Messaging;

namespace Tangosol.Net
{
    /// <summary>
    /// An <b>IInvocable</b> object is a portable object that can be invoked on the
    /// remote cluster member to which the client is connected and can 
    /// optionally register a return value for the invocation.
    /// </summary>
    /// <remarks>
    /// When an IInvocable object is received for execution, the order of
    /// execution is:
    /// <list type="bullet">
    /// <item>
    /// The <b>IInvocable</b> object is deserialized by the
    /// <see cref="IInvocationService"/>.
    /// </item>
    /// <item>
    /// The <b>IInvocationService</b> provides a reference to itself to the
    /// IInvocable object by calling the init method.
    /// </item>
    /// <item>
    /// The <b>IInvocationService</b> invokes the IInvocable object by calling
    /// the run method.
    /// </item>
    /// <item>
    /// If the <b>IInvocationService</b> is responsible for returning a value
    /// from the invocation, it obtains the value from the Result property.
    /// </item>
    /// </list>
    /// </remarks>
    /// <author>Cameron Purdy  2003.01.05</author>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    /// <since>Coherence 2.1</since>
    public interface IInvocable : IRunnable, IPortableObject
    {
        /// <summary>
        /// Called by the <see cref="IInvocationService"/> exactly once on
        /// this IInvocable object as part of its initialization.
        /// </summary>
        /// <remarks>
        /// <b>Note:</b> implementations of the IInvocable interface that
        /// store the service reference must do so only in a transient field.
        /// </remarks>
        /// <param name="service">
        /// The containing <b>InvocationService</b>.
        /// </param>
        void Init(IInvocationService service);

        /// <summary>
        /// The result from the invocation of this object.
        /// </summary>
        /// <remarks>
        /// This property value is read after the <see cref="IRunnable.Run"/>
        /// method returns.
        /// </remarks>
        /// <value>
        /// The object representing result from the invocation.
        /// </value>
        object Result
        {
            get;
        }
    }
}