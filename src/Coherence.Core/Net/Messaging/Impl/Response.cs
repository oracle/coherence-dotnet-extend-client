/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.IO;
using System.Text;
using Tangosol.IO.Pof;
using Tangosol.Util.Collections;

namespace Tangosol.Net.Messaging.Impl
{
    /// <summary>
    /// Base implementation of <see cref="IResponse"/>.
    /// </summary>
    /// <author>Ana Cikic  2006.08.18</author>
    /// <seealso cref="IResponse"/>
    /// <seealso cref="Message"/>
    /// <seealso cref="Request"/>
    public abstract class Response : Message, IResponse
    {
        #region IResponse implementation

        /// <summary>
        /// The unique identifier of the <b>IRequest</b> for which this
        /// IResponse is being sent.
        /// </summary>
        /// <value>
        /// The unique identifier of the <b>IRequest</b> associated with this
        /// IResponse.
        /// </value>
        public virtual long RequestId
        {
            get { return m_requestId; }
            set { m_requestId = value; }
        }

        /// <summary>
        /// Determine if an exception occured while processing the
        /// <b>IRequest</b>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If this property value is <b>false</b>, the result of processing
        /// the <b>IRequest</b> can be determined by reading the
        /// <see cref="Result"/> property value.</p>
        /// <p>
        /// If this property value is <b>true</b>, <see cref="Result"/> may
        /// return the cause of the failure (in the form of an
        /// <b>Exception</b> object).</p>
        /// </remarks>
        /// <value>
        /// <b>false</b> if the <b>IRequest</b> was processed successfully;
        /// <b>true</b> if an exception occured while processing the
        /// <b>IRequest</b>.
        /// </value>
        public virtual bool IsFailure
        {
            get { return m_isFailure; }
            set { m_isFailure = value; }
        }

        /// <summary>
        /// The result of processing the <b>IRequest</b>.
        /// </summary>
        /// <value>
        /// The result of processing the <b>IRequest</b> associated with
        /// this IResponse.
        /// </value>
        public virtual object Result
        {
            get { return m_result; }
            set
            {
                m_result     = value;
                ResultFormat = ResultFormatType.Generic;
            }
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
        public override void ReadExternal(IPofReader reader)
        {
            base.ReadExternal(reader);

            RequestId = reader.ReadInt64(0);
            IsFailure = reader.ReadBoolean(1);

            // determine which result format is being used
            ResultFormatType format = (ResultFormatType) reader.ReadInt32(2);
            ResultFormat = format;

            switch (format)
            {
                default:
                case ResultFormatType.Generic:
                    Result = reader.ReadObject(3);
                    break;

                case ResultFormatType.Collection:
                    ICollection collection = reader.ReadCollection(4, null);
                    Result = collection;
                    break;

                case ResultFormatType.Map:
                    IDictionary map = reader.ReadDictionary(5, new HashDictionary());
                    Result = map;
                    break;
            }
        }

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
        public override void WriteExternal(IPofWriter writer)
        {
            base.WriteExternal(writer);

            writer.WriteInt64(0, RequestId);
            writer.WriteBoolean(1, IsFailure);

            ResultFormatType format = ResultFormat;
            writer.WriteInt32(2, (int) format);

            switch (format)
            {
                default:
                case ResultFormatType.Generic:
                    writer.WriteObject(3, Result);
                    break;

                case ResultFormatType.Collection:
                    writer.WriteCollection(4, (ICollection) Result);
                    break;

                case ResultFormatType.Map:
                    writer.WriteDictionary(5, (IDictionary) Result);
                    break;
            }
        }

        #endregion
        
        #region Message Overrides

        /// <inheritdoc />
        protected override string GetDescription()
        {
            StringBuilder sb = new StringBuilder(base.GetDescription());

            sb.Append(", RequestId=").Append(RequestId);
            if (IsFailure)
            {
                sb.Append(", Failure=");
                sb.Append(Result);
            }
            else
            {
                sb.Append(", Result=");
                object oResult = Result;
                sb.Append(oResult == null ? "null" : oResult.GetType().Name + "(HashCode=" + oResult.GetHashCode() + ')');
            }
            
            return sb.ToString();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// The POF format of the result.
        /// </summary>
        /// <value>
        /// One of the <see cref="ResultFormatType"/> values.
        /// </value>
        protected internal virtual ResultFormatType ResultFormat
        {
            get { return m_resultFormat; }
            set { m_resultFormat = value; }
        }

        /// <summary>
        /// Set the result of processing the <b>Request</b> as a
        /// <b>ICollection</b>.
        /// </summary>
        /// <param name="result">
        /// Collection representing the result.
        /// </param>
        public virtual void SetResultAsCollection(ICollection result)
        {
            Result       = result;
            ResultFormat = ResultFormatType.Collection;
        }

        /// <summary>
        /// Set the result of processing the <b>Request</b> as a
        /// <b>IDictionary</b>.
        /// </summary>
        /// <param name="result">
        /// Dictionary representing the result.
        /// </param>
        public virtual void SetResultAsEntrySet(IDictionary result)
        {
            Result       = (result == null ? null : new HashDictionary(result));
            ResultFormat = ResultFormatType.Map;
        }

        #endregion

        #region Data Members

        /// <summary>
        /// The status of the Response.
        /// </summary>
        private bool m_isFailure;

        /// <summary>
        /// The unique identifier of the Request associated with this
        /// Response.
        /// </summary>
        private long m_requestId;

        /// <summary>
        /// The result of processing the Request associated with this
        /// Response.
        /// </summary>
        private object m_result;

        /// <summary>
        /// The POF format of the result.
        /// </summary>
        private ResultFormatType m_resultFormat;

        #endregion

        #region Enum: ResultFormatType

        /// <summary>
        /// The types of POF format of the result.
        /// </summary>
        protected internal enum ResultFormatType
        {
            /// <summary>
            /// Result POF format: Generic.
            /// </summary>
            Generic = 0,

            /// <summary>
            /// Result POF format: Collection.
            /// </summary>
            Collection = 1,

            /// <summary>
            /// Result POF format: Map.
            /// </summary>
            Map = 2
        }

        #endregion
    }
}