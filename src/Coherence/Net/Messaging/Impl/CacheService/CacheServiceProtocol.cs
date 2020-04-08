/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging.Impl.CacheService
{
    /// <summary>
    /// The CacheService Protocol is used to obtain, release, and destroy
    /// remote references to a NamedCache running within a Coherence cluster.
    /// </summary>
    /// <author>Ivan Cikic  2006.08.25</author>
    /// <seealso cref="Protocol"/>
    public class CacheServiceProtocol : Protocol
    {
        #region Static initializer

        /// <summary>
        /// Static initializer.
        /// </summary>
        static CacheServiceProtocol()
        {
            m_instance = new CacheServiceProtocol();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The singleton CacheServiceProtocol instance.
        /// </summary>
        /// <value>
        /// The singleton CacheServiceProtocol instance.
        /// </value>
        public static CacheServiceProtocol Instance
        {
            get { return m_instance; }
            set { m_instance = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected CacheServiceProtocol()
        {
            CurrentVersion   = 1;
            SupportedVersion = 1;
        }

        #endregion

        #region CacheServiceFactory related methods

        /// <summary>
        /// Instantiate a new <see cref="CacheServiceFactory"/> for the given
        /// version of this CacheServiceProtocol.
        /// </summary>
        /// <param name="version">
        /// The version of the CacheServiceProtocol that the returned
        /// <b>CacheServiceFactory</b> will use.
        /// </param>
        /// <returns>
        /// A new <b>CacheServiceFactory</b> for the given version of this
        /// CacheServiceProtocol.
        /// </returns>
        protected override MessageFactory InstantiateMessageFactory(int version)
        {
            return new CacheServiceFactory();
        }

        #endregion

        #region Data members

        /// <summary>
        /// The singleton CacheServiceProtocol instance.
        /// </summary>
        private static CacheServiceProtocol m_instance;

        #endregion
    }
}