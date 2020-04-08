/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// IResponse is the root interface for all response messages sent by
    /// peer endpoints through an <see cref="IChannel"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// IResponse objects are created by an <see cref="IMessageFactory"/> and
    /// are associated with a corresponding <see cref="IRequest"/>. The
    /// status of the IResponse may be determined by reading the
    /// <see cref="IsFailure"/> property value:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// if <b>false</b>, the <b>IRequest</b> was successfully processed by
    /// the receiver and the IResponse object contains a valid result
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// if <b>true</b>, an exception occured while processing the
    /// <b>IRequest</b>
    /// </description>
    /// </item>
    /// </list></p>
    /// </remarks>
    /// <author>Jason Howes  2006.04.05</author>
    /// <author>Goran Milosavljevic  2006.08.15</author>
    /// <seealso cref="IMessageFactory"/>
    /// <seealso cref="IMessage"/>
    /// <seealso cref="IRequest"/>
    /// <since>Coherence 3.2</since>
    public interface IResponse : IMessage
    {
        /// <summary>
        /// The unique identifier of the <b>IRequest</b> for which this
        /// IResponse is being sent.
        /// </summary>
        /// <value>
        /// The unique identifier of the <b>IRequest</b> associated with this
        /// IResponse.
        /// </value>
        long RequestId
        {
            get;
            set;
        }

        /// <summary>
        /// Determine if an exception occured while processing the
        /// <b>IRequest</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If this property value is <b>false</b>, the result of processing
        /// the <b>IRequest</b> can be determined by reading the
        /// <see cref="Result"/> property value.</p>
        /// <p>
        /// If this property value is <b>true</b>, <see cref="Result"/> may
        /// return the cause of the failure (in the form of an
        /// <b>Exception</b> object).</p>
        /// </remarks>
        /// <value>
        /// <b>false</b> if the <b>IRequest</b> was processed successfully;
        /// <b>true</b> if an exception occured while processing the
        /// <b>IRequest</b>.
        /// </value>
        bool IsFailure
        {
            get;
            set;
        }

        /// <summary>
        /// The result of processing the <b>IRequest</b>.
        /// </summary>
        /// <value>
        /// The result of processing the <b>IRequest</b> associated with
        /// this IResponse.
        /// </value>
        object Result
        {
            get;
            set;
        }
    }
}