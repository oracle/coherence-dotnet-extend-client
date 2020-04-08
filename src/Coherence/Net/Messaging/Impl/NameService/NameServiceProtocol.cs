/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿namespace Tangosol.Net.Messaging.Impl.NameService
{
    /// <summary>
    /// The NameService Protocol is used to access a remote NameService 
    /// running within a Coherence cluster.
    /// </summary>
    /// <author>Wei Lin  2012.05.23</author>
    /// <since>Coherence 12.1.2</since>
    public class NameServiceProtocol : Protocol
    {
        #region Static initializer

        /// <summary>
        /// Static initializer.
        /// </summary>
        static NameServiceProtocol()
        {
            m_instance = new NameServiceProtocol();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The singleton NameServiceProtocol instance.
        /// </summary>
        /// <value>
        /// The singleton NameServiceProtocol instance.
        /// </value>
        public static NameServiceProtocol Instance
        {
            get { return m_instance; }
            set { m_instance = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected NameServiceProtocol()
        {
            CurrentVersion = 1;
            SupportedVersion = 1;
        }

        #endregion

        #region NameServiceProtocol related methods

        /// <summary>
        /// Instantiate a new <see cref="NameServiceFactory"/> for the
        /// given version of this NameServiceProtocol.
        /// </summary>
        /// <param name="version">
        /// The version of the NameServiceProtocol that the returned
        /// <b>NameServiceFactory</b> will use.
        /// </param>
        /// <returns>
        /// A new <b>NameServiceFactory</b> for the given version of
        /// this NameServiceProtocol.
        /// </returns>
        protected override MessageFactory InstantiateMessageFactory(int version)
        {
            return new NameServiceFactory();
        }

        #endregion

        #region Data members

        /// <summary>
        /// The singleton NameServiceProtocol instance.
        /// </summary>
        private static NameServiceProtocol m_instance;

        #endregion
    }
}
