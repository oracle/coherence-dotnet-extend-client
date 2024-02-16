/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// The NamedCache Protocol is used to manipulate a remote NamedCache
    /// running within a Coherence cluster.
    /// </summary>
    /// <author>Ivan Cikic  2006.08.25</author>
    /// <seealso cref="Protocol"/>
    public class NamedCacheProtocol : Protocol
    {
        #region Static initializer

        /// <summary>
        /// Static initializer.
        /// </summary>
        static NamedCacheProtocol()
        {
            m_instance = new NamedCacheProtocol();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The singleton NamedCacheProtocol instance.
        /// </summary>
        /// <value>
        /// The singleton NamedCacheProtocol instance.
        /// </value>
        public static NamedCacheProtocol Instance
        {
            get { return m_instance; }
            set { m_instance = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected NamedCacheProtocol()
        {
            CurrentVersion   = 10;
            SupportedVersion = 2;
        }

        #endregion

        #region NamedCacheFactory related methods

        /// <summary>
        /// Instantiate a new <see cref="NamedCacheFactory"/> for the given
        /// version of this NamedCacheProtocol.
        /// </summary>
        /// <param name="version">
        /// The version of the NamedCacheProtocol that the returned
        /// <b>NamedCacheFactory</b> will use.
        /// </param>
        /// <returns>
        /// A new <b>NamedCacheFactory</b> for the given version of this
        /// NamedCacheProtocol.
        /// </returns>
        protected override MessageFactory InstantiateMessageFactory(int version)
        {
            return new NamedCacheFactory();
        }

        #endregion

        #region Data members

        /// <summary>
        /// The singleton NamedCacheProtocol instance.
        /// </summary>
        private static NamedCacheProtocol m_instance;

        #endregion
    }
}