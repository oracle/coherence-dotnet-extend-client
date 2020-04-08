/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.IO;
using System.Reflection;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// Codecs is a container for accessing default ICodec implementations.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public class Codecs
    {
        #region Factory methods

        /// <summary>
        /// Based on the <see cref="Type"/> <c>type</c> provided either
        /// instantiate if it is unknown or use a default codec.
        /// </summary>
        /// <param name="type">
        /// The class defining the codec to use.
        /// </param>
        /// <returns>
        /// ICodec that supports encoding and decoding of objects of the 
        /// specified type.
        /// </returns>
        public static ICodec GetCodec(Type type)
        {
            ICodec codec;
            if (typeof(DefaultCodec).Equals(type) || !typeof(ICodec).IsAssignableFrom(type))
            {
                codec = DEFAULT_CODEC;
            }
            else
            {
                ConstructorInfo ci = type.GetConstructor(new Type[0]);
                codec = ci.Invoke(null) as ICodec;    
            }
            return codec;
        }

        #endregion

        #region Inner class: AbstractCodec

        /// <summary>
        /// Abstract <see cref="ICodec"/> implementations that encodes 
        /// objects by simply delegating to 
        /// <see cref="IPofWriter.WriteObject"/>. Generally the default 
        /// <c>WriteObject</c> implementation does not need to be modified as
        /// the current accommodation of types and conversion to POF is 
        /// generally accepted, with the deserialization being more likely 
        /// to be specific.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        public abstract class AbstractCodec : ICodec
        {
            #region ICodec implementation

            /// <summary>
            /// Serialize an object using the provided 
            /// <see cref="IPofWriter"/>.
            /// </summary>
            /// <param name="writer">
            /// The <see cref="IPofWriter"/>to read from.
            /// </param>
            /// <param name="index">
            /// The index of the POF property to serialize.
            /// </param>
            /// <param name="value">
            /// The value to serialize.
            /// </param>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public virtual void Encode(IPofWriter writer, int index, object value)
            {
                writer.WriteObject(index, value);
            }

            /// <summary>
            /// Deserialize an object from the provided 
            /// <see cref="IPofReader"/>. Implementing this interface allows 
            /// introducing specific return implementations. 
            /// </summary>
            /// <param name="reader">
            /// The <see cref="IPofReader"/> to read from.
            /// </param>    
            /// <param name="index">
            /// The index of the POF property to deserialize.
            /// </param>
            /// <returns>
            /// A specific implementation of the POF property.
            /// </returns>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public abstract object Decode(IPofReader reader, int index);

            #endregion
        }

        #endregion

        #region Inner class: DefaultCodec

        /// <summary>
        /// Implementation of <see cref="ICodec"/> that simply delegates to
        /// <see cref="IPofReader.ReadObject"/> and
        /// <see cref="IPofWriter.WriteObject"/> to deserialize and serialize
        /// an object.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <since>Coherence 3.7.1</since>
        public class DefaultCodec : AbstractCodec
        {
            #region ICodec implementation

            /// <summary>
            /// Deserialize an object from the provided 
            /// <see cref="IPofReader"/>. Implementing this interface allows 
            /// introducing specific return implementations. 
            /// </summary>
            /// <param name="reader">
            /// The <see cref="IPofReader"/> to read from.
            /// </param>    
            /// <param name="index">
            /// The index of the POF property to deserialize.
            /// </param>
            /// <returns>
            /// A specific implementation of the POF property.
            /// </returns>
            /// <exception cref="IOException">
            /// If an I/O error occurs.
            /// </exception>
            public override object Decode(IPofReader reader, int index)
            {
                return reader.ReadObject(index);
            }

            #endregion
        }

        #endregion

        #region Constants

        /// <summary>
        /// A singleton instance of a <see cref="DefaultCodec"/>.
        /// </summary>
        public static readonly ICodec DEFAULT_CODEC = new DefaultCodec();

        #endregion
    }
}