/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.Diagnostics;

using Tangosol.Net.Impl;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// Base implementation of <see cref="IProtocol"/>.
    /// </summary>
    /// <author>Ana Cikic  2006.08.21</author>
    /// <seealso cref="IProtocol"/>
    public abstract class Protocol : Extend, IProtocol
    {
        #region Properties

        /// <summary>
        /// Determine the newest protocol version supported by this Protocol.
        /// </summary>
        /// <value>
        /// The version number of this Protocol.
        /// </value>
        /// <seealso cref="IProtocol.CurrentVersion"/>
        public virtual int CurrentVersion
        {
            get { return m_currentVersion; }
            set { m_currentVersion = value; }
        }

        /// <summary>
        /// A map of <see cref="MessageFactory"/> objects, keyed by Protocol
        /// version.
        /// </summary>
        /// <value>
        /// A map of <b>MessageFactory</b> objects.
        /// </value>
        protected virtual IDictionary MessageFactoryMap
        {
            get { return m_messageFactoryMap; }
            set { m_messageFactoryMap = value; }
        }

        /// <summary>
        /// Gets the unique name of this Protocol.
        /// </summary>
        /// <value>
        /// The Protocol name.
        /// </value>
        /// <seealso cref="IProtocol.Name"/>
        public virtual string Name
        {
            get
            {
                string name = m_name;
                if (name == null)
                {
                    m_name = name = GetType().Name;
                }

                return name;
            }
            set { m_name = value; }
        }

        /// <summary>
        /// Determine the oldest protocol version supported by this Protocol.
        /// </summary>
        /// <value>
        /// The oldest protocol version that this Protocol object supports.
        /// </value>
        /// <seealso cref="IProtocol.SupportedVersion"/>
        public virtual int SupportedVersion
        {
            get { return m_supportedVersion; }
            set { m_supportedVersion = value; }
        }

        #endregion

        #region MessageFactory related methods

        /// <summary>
        /// Instantiate a new <b>MessageFactory</b> for the given version of
        /// this Protocol.
        /// </summary>
        /// <param name="version">
        /// The version of the Protocol that the returned
        /// <b>MessageFactory</b> will use.
        /// </param>
        /// <returns>
        /// A new <b>MessageFactory</b> for the given version of this
        /// Protocol.
        /// </returns>
        protected virtual MessageFactory InstantiateMessageFactory(int version)
        {
            return null;
        }

        /// <summary>
        /// Return a <b>MessageFactory</b> that can be used to create
        /// <see cref="Message"/> objects for the specified version of this
        /// Protocol.
        /// </summary>
        /// <param name="version">
        /// The desired Protocol version.
        /// </param>
        /// <returns>
        /// A <b>MessageFactory</b> that can create <b>Message</b> objects
        /// for the specified version of this Protocol.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified protocol version is not supported by this
        /// Protocol.
        /// </exception>
        /// <seealso cref="IProtocol.GetMessageFactory"/>
        public virtual IMessageFactory GetMessageFactory(int version)
        {
            lock (this)
            {
                if (version < SupportedVersion || version > CurrentVersion)
                {
                    throw new ArgumentException("protocol " + Name + " does not support version " + version);
                }

                IDictionary map = MessageFactoryMap;
                if (map == null)
                {
                    MessageFactoryMap = map = new HashDictionary();
                }

                MessageFactory factory;
                if (map.Contains(version))
                {
                    factory = (MessageFactory) map[version];
                }
                else
                {
                    factory = InstantiateMessageFactory(version);
                    factory.Protocol = this;
                    factory.Version  = version;

                    Debug.Assert(factory.Version == version);
                    map.Add(version, factory);
                }

                return factory;
            }
        }

        #endregion

        #region Extend override methods

        /// <summary>
        /// Return a human-readable description of this class.
        /// </summary>
        /// <returns>
        /// A string representation of this class.
        /// </returns>
        /// <since>12.2.1.3</since>
        protected override string GetDescription()
        {
            return "Versions=[" + SupportedVersion
                + ".." + CurrentVersion + ']';
        }

        #endregion

        #region Data members

        /// <summary>
        /// A map of MessageFactory objects, keyed by Protocol version.
        /// </summary>
        [NonSerialized]
        private IDictionary m_messageFactoryMap;

        /// <summary>
        /// The name of the Protocol.
        /// </summary>
        private string m_name;

        /// <summary>
        /// The newest protocol version supported by this Protocol.
        /// </summary>
        private int m_currentVersion;

        /// <summary>
        /// The oldest protocol version supported by this Protocol.
        /// </summary>
        private int m_supportedVersion;

        #endregion
    }
}