/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Threading;

using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl.InvocationService;
using Tangosol.Run.Xml;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// <see cref="IInvocationService"/> implementation that allows a usage
    /// of a remote <b>IInvocationService</b> without having to join the
    /// cluster.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.11.08</author>
    public class RemoteInvocationService : RemoteService, IInvocationService
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public RemoteInvocationService()
        {
            ServiceVersion = "3.2";
        }

        #endregion

        #region IInvocationService implementation

        /// <summary>
        /// Synchronously invoke the specified task on each of the specified
        /// members.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This method will not return until the specified members have
        /// completed their processing, failed in their processing, or died
        /// trying.</p>
        /// <p>
        /// <b>IMember</b>s that are specified but are not currently running
        /// the <b>IInvocationService</b> will not invoke the specified
        /// <see cref="IInvocable"/> object.</p>
        /// <p>
        /// <b>IMember</b>s that leave (gracefully or otherwise) before the
        /// invocation completes will not register a result, and the amount
        /// of processing that completed is indeterminate. <b>IMember</b>s
        /// that encounter an exception during invocation will not be
        /// retried; whatever result has been registered by that point by
        /// that member for that <b>IInvocable</b> object will be returned.
        /// Specifically, the result for a given member will be null under
        /// the following conditions:</p>
        /// <list type="bullet">
        /// <item>if the member did not exist</item>
        /// <item>if the member was not running the service at the time that
        /// the query method was invoked</item>
        /// <item>if the member left (via the shutdown or stop methods, or
        /// unexpectedly) before responding</item>
        /// <item>if the member encountered an exception while processing
        /// and had not registered a non-null result</item>
        /// <item>if the member completed successfully but registered no
        /// result</item>
        /// <item>if the member completed successfully but explicitly
        /// registered a result of null</item>
        /// </list>
        /// </remarks>
        /// <param name="task">
        /// The <b>IInvocable</b> object to distribute to the specified
        /// members in order to be invoked on those members.
        /// </param>
        /// <param name="col">
        /// Optional collection of cluster members to which the
        /// <b>IInvocable</b> object will be distributed; if <c>null</c>, the
        /// <b>IInvocable</b> object will be distributed to all cluster
        /// members that are running this service.
        /// </param>
        /// <returns>
        /// An <b>IDictionary</b> of result objects keyed by <see cref="IMember"/>
        /// object.
        /// </returns>
        public virtual IDictionary Query(IInvocable task, ICollection col)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task cannot be null.");
            }

            if (col != null)
            {
                throw new ArgumentException("directed query not supported; " +
                    "the specified IMember collection must be null.");
            }

            IChannel          channel = EnsureChannel();
            IMessageFactory   factory = channel.MessageFactory;
            InvocationRequest request = (InvocationRequest) factory.CreateMessage(InvocationRequest.TYPE_ID);

            request.Task = task;

            object  result = channel.Request(request);
            IMember member = OperationalContext.LocalMember;

            IDictionary resultDictionary = new HashDictionary();
            resultDictionary.Add(member, result);
            return resultDictionary;
        }

        #endregion

        #region RemoteService override methods

        /// <summary>
        /// Open an <b>IChannel</b> to the remote ProxyService.
        /// </summary>
        protected override IChannel OpenChannel()
        {
            LookupProxyServiceAddress();

            IConnection connection = Initiator.EnsureConnection();
            return connection.OpenChannel(InvocationServiceProtocol.Instance,
                                          "InvocationServiceProxy",
                                          null,
                                          Thread.CurrentPrincipal);
        }

        /// <summary>
        /// Gets the type of the <see cref="IService"/>.
        /// </summary>
        /// <value>
        /// The type of the <b>IService</b>.
        /// </value>
        /// <since>Coherence 2.0</since>
        public override ServiceType ServiceType
        {
            get { return Net.ServiceType.RemoteInvocation; }
        }

        /// <summary>
        /// The <see cref="RemoteService.Configure"/> implementation method.
        /// </summary>
        /// <remarks>
        /// This method must only be called by a thread that has synchronized
        /// on this RemoteService.
        /// </remarks>
        /// <param name="xml">
        /// The <b>IXmlElement</b> containing the new configuration for this
        /// RemoteService.
        /// </param>
        protected override void DoConfigure(IXmlElement xml)
        {
            base.DoConfigure(xml);

            // register all Protocols
            IConnectionInitiator initiator = Initiator;
            initiator.RegisterProtocol(InvocationServiceProtocol.Instance);
        }

        #endregion
    }
}