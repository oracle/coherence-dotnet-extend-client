/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;

using Tangosol.Net.Impl;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// Base implementation of <see cref="IMessageFactory"/>.
    /// </summary>
    /// <author>Ana Cikic  2006.08.19</author>
    /// <seealso cref="IMessageFactory"/>
    /// <seealso cref="IMessage"/>
    public class MessageFactory : Extend, IMessageFactory
    {
        #region Properties

        /// <summary>
        /// The <see cref="IProtocol"/> for which this MessageFactory creates
        /// <see cref="IMessage"/> objects.
        /// </summary>
        /// <value>
        /// The <b>IProtocol</b> associated with this MessageFactory.
        /// </value>
        public virtual IProtocol Protocol
        {
            get { return m_protocol; }
            set { m_protocol = value; }
        }

        /// <summary>
        /// Gets the <see cref="IProtocol"/> version supported by this
        /// MessageFactory.
        /// </summary>
        /// <value>
        /// The <b>IProtocol</b> version associated with this MessageFactory.
        /// </value>
        public virtual int Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        /// <summary>
        /// An array of classes that are subclasses of the <b>Message</b> and
        /// that can be created by this MessageFactory.
        /// </summary>
        /// <remarks>
        /// An array is indexed by message type identifiers
        /// (see <see cref="Message.TypeId"/>).<p/>
        /// It needs to be initialized for each MessageFactory implementation
        /// (<see cref="InitializeMessageTypes"/>).
        /// </remarks>
        /// <value>
        /// An array of classes that are subclasses of the <b>Message</b>.
        /// </value>
        protected virtual Type[] MessageTypesArray
        {
            get { return m_messageTypesArray; }
            set { m_messageTypesArray = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MessageFactory()
        {
        }

        #endregion

        #region Message creation

        /// <summary>
        /// Type of the class that is subclass of the <b>Message</b> with
        /// specified type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The type identifier of class that is subclass of the
        /// <b>Message</b>.
        /// </param>
        protected virtual Type GetMessageType(int typeId)
        {
            Type[] types = MessageTypesArray;
            return types != null && typeId < types.Length ? types[typeId] : null;
        }

        /// <summary>
        /// Adds class that is subclass of the <b>Message</b> to the array of
        /// subclasses that can be created by this MessageFactory.
        /// </summary>
        /// <param name="typeId">
        /// Array index at which class should be inserted. It is also type
        /// identifier.
        /// </param>
        /// <param name="cls">
        /// Class to be inserted into array of subclasses.
        /// </param>
        protected virtual void SetMessageType(int typeId, Type cls)
        {
            Debug.Assert(cls != null);

            Type[] types = MessageTypesArray;
            if (types == null || typeId >= types.Length)
            {
                // resize, making the array bigger than necessary (avoid resizes)
                Type[] typesNew = new Type[Math.Max(typeId + (NumberUtils.URShift(typeId, 1)), typeId + 4)];

                // copy original data
                if (types != null)
                {
                    types.CopyTo(typesNew, 0);
                }
                MessageTypesArray = types = typesNew;
            }
            types[typeId] = cls;
        }

        /// <summary>
        /// Initialize an array with subclasses of <b>Message</b> that can be
        /// created by this MessageFactory so that array is indexed by the
        /// type identifier.
        /// </summary>
        /// <remarks>
        /// An array is indexed by the <b>Message</b> type identifiers.
        /// </remarks>
        /// <param name="types">
        /// An array containing all subclasses, not indexed in any particular
        /// manner.
        /// </param>
        protected virtual void InitializeMessageTypes(Type[] types)
        {
            Type clsMessage = typeof(Message);

            if (types != null)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    Type cls = types[i];
                    if (clsMessage.IsAssignableFrom(cls))
                    {
                        try
                        {
                            Message message = (Message) Activator.CreateInstance(cls);
                            int     type    = message.TypeId;
                            if (GetMessageType(type) != null)
                            {
                                throw new ArgumentException("duplicate message type "
                                    + type + ": "
                                    + cls + ", "
                                    + GetMessageType(type));
                            }
                            SetMessageType(type, cls);
                        }
                        catch (Exception e)
                        {
                            CacheFactory.Log("Unable to instantiate a message of type \"" + cls + '"',
                                             e, CacheFactory.LogLevel.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a new <b>Message</b> object of the specified type.
        /// </summary>
        /// <param name="type">
        /// The type identifier of the <b>Message</b> class to instantiate.
        /// </param>
        /// <returns>
        /// The new <b>Message</b> object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified type is unknown to this MessageFactory.
        /// </exception>
        public virtual IMessage CreateMessage(int type)
        {
            Type cls = GetMessageType(type);
            if (cls == null)
            {
                throw new ArgumentException("Unable to instantiate a Message of type: " + type);
            }

            Message message = (Message) Activator.CreateInstance(cls);

            // set the Message version
            message.ImplVersion = Version;

            return message;
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
            return "Protocol=" + Protocol + ", Version=" + Version;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The IProtocol for which this MessageFactory creates Message
        /// objects.
        /// </summary>
        private IProtocol m_protocol;

        /// <summary>
        /// The IProtocol version supported by this MessageFactory.
        /// </summary>
        private int m_version;

        /// <summary>
        /// An array of Message classes that can be created by this
        /// MessageFactory indexed by Message type identifier.
        /// </summary>
        [NonSerialized]
        private Type[] m_messageTypesArray;

        #endregion
    }
}