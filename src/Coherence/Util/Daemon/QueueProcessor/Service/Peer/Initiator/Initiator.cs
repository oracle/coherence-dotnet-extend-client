/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.Security.Principal;

using Tangosol.Net;
using Tangosol.Net.Messaging;
using Tangosol.Net.Messaging.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util.Collections;

namespace Tangosol.Util.Daemon.QueueProcessor.Service.Peer.Initiator
{
    /// <summary>
    /// Base implementation of an <see cref="IConnectionInitiator"/>.
    /// </summary>
    public abstract class Initiator : Peer, IConnectionInitiator
    {
        #region Properties

        /// <summary>
        /// A set of <see cref="IDisposable"/> to close on exit.
        /// </summary>
        /// <since>12.2.1</since>
        public virtual SafeHashSet CloseOnExit
        {
            get
            {
                return f_setCloseOnExit;
            }
        }

        /// <summary>
        /// The <see cref="Connection"/> managed by this
        /// IConnectionInitiator.
        /// </summary>
        /// <value>
        /// The <b>Connection</b> managed by this IConnectionInitiator.
        /// </value>
        public virtual Connection Connection { get; protected set; }

        /// <summary>
        /// The maximum amount of time (in milliseconds) that the
        /// IConnectionInitiator will wait for a new <b>Connection</b> to be
        /// established.
        /// </summary>
        /// <remarks>
        /// If 0, the IConnectionInitiator will wait indefinitely. This
        /// property defaults to the value of the
        /// <see cref="Peer.RequestTimeout"/>.
        /// </remarks>
        /// <value>
        /// The maximum amount of time (in milliseconds) that the
        /// IConnectionInitiator will wait for a new <b>Connection</b> to be
        /// established.
        /// </value>
        public virtual long ConnectTimeout { get; set; }

        /// <summary>
        /// The <see cref="IProtocol"/> understood by the IReceiver.
        /// </summary>
        /// <remarks>
        /// Only <b>IChannel</b> objects with the specified <b>IProtocol</b>
        /// can be registered with this IReceiver.
        /// </remarks>
        /// <value>
        /// The <b>IProtocol</b> used by this IReceiver.
        /// </value>
        public override IProtocol Protocol
        {
            get
            {
                IProtocol protocol = m_protocol;
                if (protocol == null)
                {
                    Protocol = (protocol = MessagingProtocol.Instance);
                }
                return protocol;
            }
            set
            {
                m_protocol = value;
            }
        }

        #endregion

        #region Factory

        /// <summary>
        /// Factory method: create and configure a new
        /// <see cref="IConnectionInitiator"/> for the given configuration.
        /// </summary>
        /// <param name="xml">
        /// The <b>IXmlElement</b> used to create and configure a new
        /// <b>IConnectionInitiator</b>.
        /// </param>
        /// <param name="ctx">
        /// The <b>IOperationalContext</b> used to configure a new <b>Peer</b>.
        /// </param>
        /// <returns>
        /// A new <b>IConnectionInitiator</b>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the given <b>IXmlElement</b> is not a valid
        /// <b>IConnectionInitiator</b> configuration element.
        /// </exception>
        /// <seealso cref="FindInitiatorConfig"/>
        public static IConnectionInitiator CreateInitiator(IXmlElement xml, IOperationalContext ctx)
        {
            IConnectionInitiator initiator;

            if (xml.GetElement("tcp-initiator") != null)
            {
               initiator = (IConnectionInitiator) Activator.CreateInstance(typeof(TcpInitiator));
            }
            else
            {
                throw new ArgumentException("unsupported \"initiator-config\":\n" + xml);
            }

            initiator.OperationalContext = ctx;
            initiator.Configure(xml);
            return initiator;
        }

        /// <summary>
        /// Return a child <b>IXmlElement</b> of the given <b>IXmlElement</b>
        /// that can be used to  create and configure a new
        /// <b>IConnectionInitiator</b>.
        /// </summary>
        /// <remarks>
        /// The given <b>IXmlElement</b> must have a child element with one
        /// of the following names:
        /// <list type="number">
        /// <item>
        /// tcp-initiator: used to create and configure a new TcpInitiator
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="xml">
        /// The parent <b>IXmlElement</b> of the <b>IXmlElement</b> used to
        /// create and configure a new <b>IConnectionInitiator</b>.
        /// </param>
        /// <returns>
        /// A child <b>IXmlElement</b> that can be used to create and
        /// configure a new <b>IConnectionInitiator</b> or <c>null</c> if no
        /// such <b>IXmlElement</b> exists.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the given <b>IXmlElement</b> does not have a valid
        /// <b>IConnectionInitiator</b> configuration child element.
        /// </exception>
        public static IXmlElement FindInitiatorConfig(IXmlElement xml)
        {
            IXmlElement xmlConfig = xml.Name.Equals("initiator-config") 
                    ? xml : xml.GetSafeElement("initiator-config");

            if (xmlConfig.GetElement("tcp-initiator") != null)
            {
                return (IXmlElement) xmlConfig.Clone();
            }

            throw new ArgumentException("the \"initiator-config\" element is either"
                    + " missing, empty, or does not contain a valid transport-specific"
                    + " child element:\n" + xml);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Open and return a new Connection.
        /// </summary>
        /// <returns>
        /// A newly opened Connection.
        /// </returns>
        protected virtual Connection OpenConnection()
        {
            Connection connection = InstantiateConnection();
            connection.Open();

            return connection;
        }

        #endregion

        #region IConnectionInitiator implementation

        /// <summary>
        /// Create a new or return the existing <b>IConnection</b> object.
        /// </summary>
        /// <remarks>
        /// <p>
        /// An <b>IConnection</b> object has a one-way state transition from
        /// open to closed; this method will always return an open
        /// <b>IConnection</b> object. If the previously existing
        /// <b>IConnection</b> object has transitioned to a closed state,
        /// this method will return a new <b>IConnectin</b> object in the
        /// open state.</p>
        /// </remarks>
        /// <returns>
        /// An <b>IConnection</b> object representing a client's connection to
        /// a server.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// If the IConnectionInitiator is not running.
        /// </exception>
        public virtual IConnection EnsureConnection()
        {
            lock (this)
            {
                if (!IsRunning)
                {
                    throw new InvalidOperationException(ServiceName + " is not running");
                }

                Connection connection = Connection;
                if (connection == null || !connection.IsOpen)
                {
                    connection = OpenConnection();
                }

                return connection;
            }
        }

        #endregion

        #region Peer overrides

        /// <summary>
        /// Check the <see cref="Connection"/>(s) managed by this
        /// <see cref="IConnectionManager"/> for a ping timeout.
        /// </summary>
        /// <remarks>
        /// A <b>Connection</b> that has not received a
        /// <see cref="PingResponse"/> for an oustanding
        /// <see cref="PingRequest"/> within the configured
        /// <see cref="Peer.PingTimeout"/> will be closed.
        /// </remarks>
        protected override void CheckPingTimeouts()
        {
            Connection connection = Connection;
            if (connection != null)
            {
                CheckPingTimeout(connection);
            }
        }

        /// <summary>
        /// Configure the controllable service.
        /// </summary>
        /// <remarks>
        /// <p/>
        /// This method can only be called before the controllable service
        /// is started.
        /// </remarks>
        /// <param name="xml">
        /// An <see cref="IXmlElement"/> carrying configuration information
        /// specific to the IControllable object.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the service is already running.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the configuration information is invalid.
        /// </exception>
        public override void Configure(IXmlElement xml)
        {
            lock (this)
            {
                base.Configure(xml);

                ConnectTimeout = ParseTime(xml, "connect-timeout", RequestTimeout);
            }
        }

        /// <summary>
        /// Human-readable description of additional Service properties.
        /// </summary>
        /// <remarks>
        /// Used by <see cref="Service.ToString"/>.
        /// </remarks>
        public override string Description
        {
            get { return base.Description + ", ConnectTimeout=" + ConnectTimeout; }
        }

        /// <summary>
        /// Open the given <see cref="Connection"/>.
        /// </summary>
        /// <remarks>
        /// This method is called by
        /// <see cref="Net.Messaging.Impl.Connection.Open"/> and is always run
        /// on client threads.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> to open.
        /// </param>
        public override void OpenConnection(Connection connection)
        {
            Debug.Assert(connection != null);

            IChannel        channel0 = InternalChannel;
            IMessageFactory factory0 = channel0.MessageFactory;

            var request = (OpenConnection)
                factory0.CreateMessage(Util.Daemon.QueueProcessor.Service.Peer.OpenConnection.TYPE_ID);

            IPrincipal principal = System.Threading.Thread.CurrentPrincipal;

            request.ConnectionOpen = connection;
            request.IdentityToken  = SerializeIdentityToken(GenerateIdentityToken(principal));
            request.Principal      = principal;

            var status = (IStatus) channel0.Request(request);
            if (status != null)
            {
                try
                {
                    status.WaitForResponse(ConnectTimeout);
                }
                catch (RequestTimeoutException e)
                {
                    connection.Close(false, e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Called after a <see cref="Connection"/> has closed.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> that was closed.
        /// </param>
        public override void OnConnectionClosed(Connection connection)
        {
            if (Connection == connection)
            {
                Connection = null;
                base.OnConnectionClosed(connection);
            }
        }

        /// <summary>
        /// Called after a <see cref="Connection"/> is closed due to an error
        /// or exception.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> that was closed.
        /// </param>
        /// <param name="e">
        /// The reason the <b>Connection</b> was closed.
        /// </param>
        public override void OnConnectionError(Connection connection, Exception e)
        {
            if (Connection == connection)
            {
                Connection = null;
                base.OnConnectionError(connection, e);
            }
        }

        /// <summary>
        /// Called after a <see cref="Connection"/> has been successfully
        /// established.
        /// </summary>
        /// <remarks>
        /// This method is called on the service thread.
        /// </remarks>
        /// <param name="connection">
        /// The <b>Connection</b> that was opened.
        /// </param>
        public override void OnConnectionOpened(Connection connection)
        {
            if (InternalConnection == connection)
            {
                return;
            }

            if (Connection == null)
            {
                Connection = connection;
                base.OnConnectionOpened(connection);
            }
        }

        /// <summary>
        /// Event notification called right before the daemon thread
        /// terminates.
        /// </summary>
        /// <remarks>
        /// This method is guaranteed to be called only once and on the
        /// daemon's thread.
        /// </remarks>
        protected override void OnExit()
        {
            SafeHashSet setClose = CloseOnExit;
            lock(setClose.SyncRoot)
            {
                foreach (object o in setClose)
                {
                    try
                    {
                        ((IDisposable) o).Dispose();
                    }
                    catch (Exception) { }
                }
            }

            base.OnExit();
        }

        /// <summary>
        /// The default implementation of this method sets
        /// <see cref="Service.IsAcceptingClients"/> to <b>false</b>.
        /// </summary>
        protected override void OnServiceStopped()
        {
            Connection connection = Connection;
            if (connection != null)
            {
                connection.CloseInternal(true, null, 100);
                if (connection.IsOpen)
                {
                    // we were unable to close the Connection because a daemon thread is
                    // currently executing within the Connection; interrupt all daemons
                    //TODO: getDaemonPool().stop();

                    connection.CloseInternal(true, null, 1000);
                    if (connection.IsOpen)
                    {
                        CacheFactory.Log("Unable to close \"" + connection
                                + "\"; this Connection will be abandoned", CacheFactory.LogLevel.Error);
                    }
                }
            }
            base.OnServiceStopped();
        }

        /// <summary>
        /// The default implementation of this method sets
        /// <see cref="Service.IsAcceptingClients"/> to <b>false</b>.
        /// </summary>
        protected override void OnServiceStopping()
        {
            Connection connection = Connection;
            if (connection != null)
            {
                connection.CloseInternal(true, null, 0);
            }
            base.OnServiceStopping();
        }

        /// <summary>
        /// Ping the <see cref="Connection"/>(s) managed by this
        /// <b>IConnectionManager</b>.
        /// </summary>
        protected override void Ping()
        {
            Connection connection = Connection;
            if (connection != null)
            {
                connection.Ping();
            }
        }

        #endregion

        #region Data members

        /// <summary>
        /// Protocol.
        /// </summary>
        private IProtocol m_protocol;

        /// <summary>
        /// A set of <see cref="IDisposable"/> to close on exit.
        /// </summary>
        /// <since>12.2.1</since>
        private readonly SafeHashSet f_setCloseOnExit = new SafeHashSet();

        #endregion
    }
}