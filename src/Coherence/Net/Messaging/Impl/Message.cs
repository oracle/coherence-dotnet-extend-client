/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Impl;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// Base implementation of <see cref="IMessage"/>.
    /// </summary>
    /// <author>Ana Cikic  2006.08.18</author>
    /// <seealso cref="IMessage"/>
    /// <seealso cref="IEvolvablePortableObject"/>
    /// <seealso cref="IChannel"/>
    public abstract class Message : Extend, IMessage, IEvolvablePortableObject
    {
        #region IMessage implementation

        /// <summary>
        /// Return the identifier for this IMessage object's class.
        /// </summary>
        /// <remarks>
        /// The type identifier is scoped to the <b>IMessageFactory</b> that
        /// created this IMessage.
        /// </remarks>
        /// <value>
        /// An identifier that uniquely identifies this IMessage object's
        /// class.
        /// </value>
        public virtual int TypeId
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets or sets the <b>IChannel</b> through which the IMessage will
        /// be sent, was sent, or was received.
        /// </summary>
        /// <value>
        /// The <b>IChannel</b> through which the IMessage will be sent, was
        /// sent, or was received.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the <b>IChannel</b> has already been set.
        /// </exception>
        public virtual IChannel Channel
        {
            get { return m_channel; }
            set
            {
                Debug.Assert(value != null);

                if (m_channel == null)
                {
                    m_channel = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        /// <summary>
        /// Determine if this IMessage should be executed in the same order
        /// as it was received relative to other messages sent through the
        /// same <b>IChannel</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Consider two messages: M1 and M2. Say M1 is received before M2
        /// but executed on a different execute thread (for example, when the
        /// <see cref="IConnectionManager"/> is configured with an execute
        /// thread pool of size greater than 1). In this case, there is no
        /// way to guarantee that M1 will finish executing before M2.
        /// However, if M1 returns <b>true</b> from this method, the
        /// <b>IConnectionManager</b> will execute M1 on its service thread,
        /// thus guaranteeing that M1 will execute before M2.</p>
        /// <p>
        /// In-order execution should be considered as a very advanced
        /// feature and implementations that return <b>true</b> from this
        /// method must exercise extreme caution during execution, since any
        /// delay or unhandled exceptions will cause a delay or complete
        /// shutdown of the underlying <b>IConnectionManager</b>.</p>
        /// </remarks>
        /// <returns>
        /// <b>true</b> if the IMessage should be executed in the same order
        /// as it was received relative to other messages.
        /// </returns>
        public virtual bool ExecuteInOrder
        {
            get { return false; }
        }

        #endregion

        #region IEvolvable implementation

        /// <summary>
        /// Gets or sets the version associated with the data stream from
        /// which this object was deserialized.
        /// </summary>
        /// <remarks>
        /// If the object was constructed (not deserialized), the data
        /// version is the same as the implementation version.
        /// </remarks>
        /// <value>
        /// The version of the data used to initialize this object, greater
        /// than or equal to zero.
        /// </value>
        /// <exception cref="ArgumentException">
        /// If the specified version is negative.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If the object is not in a state in which the version can be set,
        /// for example outside of deserialization.
        /// </exception>
        public virtual int DataVersion
        {
            get { return m_dataVersion; }
            set { m_dataVersion = value; }
        }

        /// <summary>
        /// Gets or sets all the unknown remainder of the data stream from
        /// which this object was deserialized.
        /// </summary>
        /// <remarks>
        /// The remainder is unknown because it is data that was originally
        /// written by a future version of this object's type.
        /// </remarks>
        /// <value>
        /// Future data in binary form.
        /// </value>
        /// <exception cref="InvalidOperationException">
        /// If the object is not in a state in which the version can be set,
        /// for example outside of deserialization.
        /// </exception>
        public virtual Binary FutureData
        {
            get { return m_futureData; }
            set { m_futureData = value; }
        }

        /// <summary>
        /// Determine the serialization version supported by the implementing
        /// type.
        /// </summary>
        /// <value>
        /// The serialization version supported by this object.
        /// </value>
        public virtual int ImplVersion
        {
            get { return m_implVersion; }
            set { m_implVersion = value; }
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {}

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {}

        #endregion

        #region IRunnable implementation

        /// <summary>
        /// Execute the action specific to the Message implementation.
        /// </summary>
        public virtual void Run()
        {}

        #endregion

        #region Extend Overrides

        /// <summary>
        /// Return a human-readable description of this message.
        /// </summary>
        /// <returns>String description of this message</returns>
        /// <since>12.2.1.4.1</since>
        protected override string GetDescription()
        {
            IChannel channel  = Channel;
            String   sChannel = channel == null ? "null" : "" + channel.Id;
            
            return "Type=" + TypeId + ", Channel=" + sChannel;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The version associated with the data reader from which this
        /// object was deserialized.
        /// </summary>
        [NonSerialized]
        private int m_dataVersion;

        /// <summary>
        /// The unknown remainder of the data stream from which this object
        /// was deserialized.
        /// </summary>
        [NonSerialized]
        private Binary m_futureData;

        /// <summary>
        /// The serialization version supported by the implementing class.
        /// </summary>
        [NonSerialized]
        private int m_implVersion;

        /// <summary>
        /// The IChannel associated with this message.
        /// </summary>
        [NonSerialized]
        private IChannel m_channel;

        #endregion
    }
}