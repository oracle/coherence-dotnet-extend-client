/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging.Impl.InvocationService
{
    /// <summary>
    /// The InvocationService Protocol is used to execute Invocable tasks
    /// within a remote Coherence cluster.
    /// </summary>
    /// <author>Goran Milosavljevic  2006.09.01</author>
    public class InvocationServiceProtocol : Protocol
    {
        #region Static initializer

        /// <summary>
        /// Static initializer.
        /// </summary>
        static InvocationServiceProtocol()
        {
            m_instance = new InvocationServiceProtocol();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The singleton InvocationServiceProtocol instance.
        /// </summary>
        /// <value>
        /// The singleton InvocationServiceProtocol instance.
        /// </value>
        public static InvocationServiceProtocol Instance
        {
            get { return m_instance; }
            set { m_instance = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected InvocationServiceProtocol()
        {
            CurrentVersion   = 1;
            SupportedVersion = 1;
        }

        #endregion

        #region InvocationServiceProtocol related methods

        /// <summary>
        /// Instantiate a new <see cref="InvocationServiceFactory"/> for the
        /// given version of this InvocationServiceProtocol.
        /// </summary>
        /// <param name="version">
        /// The version of the InvocationServiceProtocol that the returned
        /// <b>InvocationServiceFactory</b> will use.
        /// </param>
        /// <returns>
        /// A new <b>InvocationServiceFactory</b> for the given version of
        /// this InvocationServiceProtocol.
        /// </returns>
        protected override MessageFactory InstantiateMessageFactory(int version)
        {
            return new InvocationServiceFactory();
        }

        #endregion

        #region Data members

        /// <summary>
        /// The singleton InvocationServiceProtocol instance.
        /// </summary>
        private static InvocationServiceProtocol m_instance;

        #endregion
    }
}
