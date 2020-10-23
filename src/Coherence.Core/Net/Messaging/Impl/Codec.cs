/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net.Impl;
using Tangosol.Util;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// The default <see cref="ICodec"/> implementation used by the
    /// <see cref="Connection"/> if one was not explicitly configured on the
    /// <see cref="IConnectionManager"/> that created the <b>Connection</b>.
    /// </summary>
    /// <author>Ana Cikic  2006.08.21</author>
    /// <seealso cref="ICodec"/>
    /// <seealso cref="Connection"/>
    /// <seealso cref="IConnectionManager"/>
    public class Codec : Extend, ICodec
    {
        #region ICodec implementation

        /// <summary>
        /// Encode and write a binary representation of the given
        /// <b>IMessage</b> to the given <see cref="DataWriter"/>.
        /// </summary>
        /// <remarks>
        /// Using the passed <see cref="IChannel"/>, the Codec has access to
        /// both the <see cref="IMessageFactory"/> for the <b>IChannel</b>
        /// and the underlying <see cref="IConnection"/>.
        /// </remarks>
        /// <param name="channel">
        /// The <b>IChannel</b> object through which the binary-encoded
        /// <b>IMessage</b> was passed.
        /// </param>
        /// <param name="message">
        /// The <b>IMessage</b> to encode.
        /// </param>
        /// <param name="writer">
        /// The <b>DataWriter</b> to write the binary representation of the
        /// <b>IMessage</b> to.
        /// </param>
        /// <exception cref="IOException">
        /// If an error occurs encoding or writing the <b>IMessage</b>.
        /// </exception>
        /// <seealso cref="ICodec.Encode"/>
        public virtual void Encode(IChannel channel, IMessage message, DataWriter writer)
        {
            Debug.Assert(channel is IPofContext);
            Debug.Assert(message is IPortableObject);

            IPofContext     context   = (IPofContext) channel;
            PofStreamWriter pofwriter = new PofStreamWriter.UserTypeWriter(writer, context, message.TypeId, 0);

            ISerializer serializer = channel.Serializer;
            if (serializer is ConfigurablePofContext)
            {
                ConfigurablePofContext pofCtx = (ConfigurablePofContext) serializer;
                if (pofCtx.IsReferenceEnabled)
                {
                    pofwriter.EnableReference();
                }
            }

            // set the version identifier
            bool       isEvolvable = message is IEvolvable;
            IEvolvable evolvable   = null;
            if (isEvolvable)
            {
                evolvable = (IEvolvable) message;
                pofwriter.VersionId = Math.Max(evolvable.DataVersion, evolvable.ImplVersion);
            }

            // write the Message properties
            ((IPortableObject) message).WriteExternal(pofwriter);

            // write the future properties
            pofwriter.WriteRemainder(isEvolvable ? evolvable.FutureData : null);
        }

        /// <summary>
        /// Reads a binary-encoded <b>IMessage</b> from the passed
        /// <see cref="DataReader"/> object.
        /// </summary>
        /// <remarks>
        /// Using the passed <see cref="IChannel"/>, the Codec has access to
        /// both the <see cref="IMessageFactory"/> for the <b>IChannel</b>
        /// and the underlying <see cref="IConnection"/>.
        /// </remarks>
        /// <param name="channel">
        /// The <b>IChannel</b> object through which the binary-encoded
        /// <b>IMessage</b> was passed.
        /// </param>
        /// <param name="reader">
        /// The <b>DataReader</b> containing the binary-encoded
        /// <b>IMessage</b>.
        /// </param>
        /// <returns>
        /// The <b>IMessage</b> object encoded in the given
        /// <b>DataReader</b>.
        /// </returns>
        /// <exception cref="IOException">
        /// If an error occurs reading or decoding the <b>IMessage</b>.
        /// </exception>
        public virtual IMessage Decode(IChannel channel, DataReader reader)
        {
            Debug.Assert(channel is IPofContext);

            IPofContext context   = (IPofContext) channel;
            int         typeId    = reader.ReadPackedInt32();
            int         versionId = reader.ReadPackedInt32();
            IPofReader  pofreader = new PofStreamReader.UserTypeReader(reader, context, typeId, versionId);
            IMessage    message   = channel.MessageFactory.CreateMessage(typeId);

            Debug.Assert(message is IPortableObject);

            // set the version identifier
            bool       isEvolvable = message is IEvolvable;
            IEvolvable evolvable   = null;
            if (isEvolvable)
            {
                evolvable = (IEvolvable) message;
                evolvable.DataVersion = versionId;
            }

            // read the Message properties
            ((IPortableObject) message).ReadExternal(pofreader);

            // read the future properties
            Binary binFuture = pofreader.ReadRemainder();
            if (isEvolvable)
            {
                evolvable.FutureData = binFuture;
            }

            return message;
        }

        #endregion

        #region Extend override methods

        /// <summary>
        /// Return a human-readable description of this Codec.
        /// </summary>
        /// <returns>
        /// A string representation of this Codec.
        /// </returns>
        /// <since>12.2.1.3</since>
        protected override string GetDescription()
        {
            return "Format=POF";
        }

        #endregion
    }
}