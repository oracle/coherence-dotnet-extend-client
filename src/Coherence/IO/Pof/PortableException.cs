/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// A <b>PortableException</b> is an exception that allows information
    /// about a remote exception to be serialized and deserialized to/from a
    /// POF stream.
    /// </summary>
    /// <author>Jason Howes  2006.08.04</author>
    /// <author>Ana Cikic  2006.08.24</author>
    [Serializable]
    public class PortableException : Exception, IPortableObject, ISerializable
    {
        #region Properties

        /// <summary>
        /// The name of the exception.
        /// </summary>
        /// <value>
        /// The name of the exception.
        /// </value>
        public virtual string Name
        {
            get
            {
                Type   type = GetType();
                string name = m_name;

                if (name != null && type == typeof(PortableException))
                {
                    string prefix = "Portable(";

                    if (name.StartsWith(prefix))
                    {
                        return name;
                    }
                    return prefix + name + ')';
                }

                return type.FullName;
            }
        }

        /// <summary>
        /// An array of strings containing the full representation of the
        /// stack trace.
        /// </summary>
        /// <remarks>
        /// The first element of the stack represents the exception's point
        /// of origin.
        /// </remarks>
        /// <value>
        /// The full stack trace.
        /// </value>
        public virtual string[] FullStackTrace
        {
            get
            {
                string[] arrStackRemote = m_arrStackRemote;
                string   stackLocal     = base.StackTrace;
                int      cLocal         = stackLocal != null ? 1 : 0;
                int      ofLocal;
                string[] arrStackFull;

                if (arrStackRemote == null)
                {
                    arrStackFull = new string[cLocal];
                    ofLocal = 0;
                }
                else
                {
                    int cRemote = arrStackRemote.Length;
                    arrStackFull = new string[cRemote + cLocal + 1];
                    Array.Copy(arrStackRemote, 0, arrStackFull, 0, cRemote);

                    arrStackFull[cRemote] = "at <process boundary>";
                    ofLocal = cRemote + 1;
                }

                if (cLocal > 0)
                {
                    arrStackFull[ofLocal] = stackLocal;
                }

                return arrStackFull;
            }
        }

        /// <summary>
        /// The <b>Exception</b> that caused this exception.
        /// </summary>
        /// <value>
        /// An exception that caused this exception.
        /// </value>
        public new virtual Exception InnerException
        {
            get { return m_innerException; }
            set { m_innerException = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PortableException()
        {
            m_name = GetType().FullName;
        }

        /// <summary>
        /// Constructs a PortableException with the specified detail message.
        /// </summary>
        /// <param name="message">
        /// The string that contains a detailed message.
        /// </param>
        public PortableException(string message) : base(message)
        {
            m_name    = GetType().FullName;
            m_message = message;
        }

        /// <summary>
        /// Construct a PortableException from an Exception object.
        /// </summary>
        /// <param name="e">
        /// The Exception object.
        /// </param>
        public PortableException(Exception e) : base(null, e)
        {
            m_name           = GetType().FullName;
            m_innerException = e;
        }

        /// <summary>
        /// Construct a PortableException from an Exception object and an
        /// additional description.
        /// </summary>
        /// <param name="message">
        /// The additional description.
        /// </param>
        /// <param name="e">
        /// The Exception object.
        /// </param>
        public PortableException(string message, Exception e) : base(message, e)
        {
            m_name           = GetType().FullName;
            m_message        = message;
            m_innerException = e;
        }

        /// <summary>
        /// Construct a PortableException class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <b>SerializationInfo</b> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <b>StreamingContext</b> that contains contextual information
        /// about the source or destination.
        /// </param>
        public PortableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            m_name           = info.GetString("Name");
            m_message        = info.GetString("PEMessage");
            m_arrStackRemote = (string[]) info.GetValue("RemoteStack", typeof(string[]));
            m_innerException = (Exception) info.GetValue("PEInnerException", typeof(Exception));
        }

        #endregion

        #region Exception overrides

        /// <summary>
        /// A message that describes the current exception.
        /// </summary>
        /// <value>
        /// The error message that explains the reason for the exception, or
        /// an empty string.
        /// </value>
        public override string Message
        {
            get { return m_message; }
        }

        /// <summary>
        /// A string representation of the frames on the call stack at the
        /// time the current exception was thrown.
        /// </summary>
        /// <value>
        /// A string that describes the contents of the call stack, with the
        /// most recent method call appearing first.
        /// </value>
        public override string StackTrace
        {
            get
            {
                StringBuilder sb           = new StringBuilder();
                string[]      arrStackFull = FullStackTrace;

                for (int i = 0, c = arrStackFull.Length; i < c; ++i)
                {
                    sb.Append("\t" + arrStackFull[i] + "\n");
                }

                Exception eCause = InnerException;
                if (eCause != null)
                {
                    sb.Append("Caused by: ");
                    sb.Append(eCause.ToString());
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Sets the <b>SerializationInfo</b> with information about the
        /// exception.
        /// </summary>
        /// <param name="info">
        /// The <b>SerializationInfo</b> that holds the serialized object
        /// data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <b>StreamingContext</b> that contains contextual information
        /// about the source or destination.
        /// </param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Name", m_name);
            info.AddValue("PEMessage", m_message);
            info.AddValue("RemoteStack", m_arrStackRemote);
            info.AddValue("PEInnerException", m_innerException);
        }

        /// <summary>
        /// Returns a string representation of the current exception.
        /// </summary>
        /// <returns>
        /// A string representation of the current exception.
        /// </returns>
        public override string ToString()
        {
            string prefix  = Name;
            string message = Message;

            return message == null ? prefix : prefix + ": " + message;
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
        {
            m_name    = reader.ReadString(0);
            m_message = reader.ReadString(1);

            ICollection coll = reader.ReadCollection(2, new ArrayList(64));
            m_arrStackRemote = new string[coll.Count];
            coll.CopyTo(m_arrStackRemote, 0);

            m_innerException = (Exception) reader.ReadObject(3);
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
        public virtual void WriteExternal(IPofWriter writer)
        {
            ExceptionPofSerializer.WriteException(writer, this);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The exception's name.
        /// </summary>
        protected string m_name;

        /// <summary>
        /// The exception's message.
        /// </summary>
        protected string m_message;

        /// <summary>
        /// A raw representation of the remote stack trace for this
        /// exception.
        /// </summary>
        protected string[] m_arrStackRemote;

        /// <summary>
        /// An Exception that caused current exception.
        /// </summary>
        protected Exception m_innerException;

        #endregion
    }
}