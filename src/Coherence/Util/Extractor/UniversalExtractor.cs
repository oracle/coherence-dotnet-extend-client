/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Universal <see cref="IValueExtractor"/> implementation.
    /// </summary>
    /// <remarks>
    /// UniversalExtractor can only run within the Coherence cluster.
    /// Refer to the Coherence for Java documentation for more information.
    /// </remarks>
    /// <author>Cameron Purdy  2002.11.01</author>
    /// <author>Gene Gleyzer  2002.11.01</author>
    /// <author>Everett Williams  2007.02.01</author>
    /// <author>Joe Fialli  2017.11.20</author>
    /// <author>Patrick Fry  2024.09.13</author>
    /// <since>14.1.2.0.0</since>
    public class UniversalExtractor : AbstractExtractor, IValueExtractor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Get the method or property name.
        /// </summary>
        /// <value>
        /// the method or property name.
        /// </value>
        public virtual string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Gets the array of arguments used to invoke the method.
        /// </summary>
        /// <value>
        /// The array of arguments used to invoke the method.
        /// </value>
        public virtual Object[] Parameters
        {
            get { return m_parameters; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor (necessary for the IPortableObject interface).
        /// </summary>
        public UniversalExtractor()
        {}

        /// <summary>
        /// Construct a <b>UniversalExtractor</b> based on a member name.
        /// </summary>
        /// <param name="name">
        /// A method or property name.
        /// </param>
        public UniversalExtractor(string name)
            : this(name, null, VALUE)
        {
        }

        /// <summary>
        /// Construct a <b>UniversalExtractor</b>.
        /// </summary>
        /// <param name="name">
        /// A method or property name.
        /// </param>
        /// <param name="parameters">
        /// The array of arguments to be used in the method invocation;
        /// may be <c>null</c>.
        /// </param>
        public UniversalExtractor(string name, object[] parameters) 
            : this(name, parameters, VALUE)
        {
        }

        /// <summary>
        /// Construct a <b>UniversalExtractor</b> based on a method name,
        /// optional parameters and the entry extraction target.
        /// </summary>
        /// <param name="name">
        /// A method or property name.
        /// </param>
        /// <param name="parameters">
        /// The array of arguments to be used in the method invocation;
        /// may be <c>null</c>.
        /// </param>
        /// <param name="target">
        /// One of the <see cref="AbstractExtractor.VALUE"/> or
        /// <see cref="AbstractExtractor.KEY"/> values
        /// </param>
        public UniversalExtractor(string name, object[] parameters, int target)
        {
            Debug.Assert(name != null);

            if (parameters != null && parameters.Length > 0 && !name.EndsWith(METHOD_SUFFIX))
            {
                throw new ArgumentException("UniversalExtractor constructor: parameter name[value:" + name + "] must end with method suffix \"" + METHOD_SUFFIX + "\" when optional parameters provided");
            }
            m_name       = name;
            m_parameters = parameters;
            m_target     = target;
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Provide a human-readable description of this
        /// <see cref="IValueExtractor"/> object.
        /// </summary>
        /// <returns>
        /// A human-readable description of this <b>IValueExtractor</b>
        /// object.
        /// </returns>
        public override string ToString()
        {
            Object[] parameters = m_parameters;
            int      cParams    = parameters == null ? 0 : parameters.Length;

            StringBuilder sb = new StringBuilder();
            if (m_target == KEY)
            {
                sb.Append(".Key");
            }
            sb.Append('.').Append(m_name).Append('(');
            for (int i = 0; i < cParams; i++)
            {
                if (i != 0)
                {
                    sb.Append(", ");
                }
                sb.Append(parameters[i]);
            }
            sb.Append(')');

            return sb.ToString();
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
            m_name       = reader.ReadString(0);
            m_parameters = (object[]) reader.ReadArray(1);
            m_target     = reader.ReadInt32(2);
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
            string name = m_name;
            if (name == null)
            {
                throw new InvalidOperationException("UniversalExtractor was constructed without a method name");
            }
            writer.WriteString(0, name);
            writer.WriteArray(1, m_parameters);
            writer.WriteInt32(2, m_target);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The name of the member to invoke.
        /// </summary>
        protected string m_name;

        /// <summary>
        /// The parameter array.
        /// </summary>
        protected Object[] m_parameters;

        /// <summary>
        /// If m_name ends with this suffix, it represents a method name.
        /// </summary>
        public static readonly string METHOD_SUFFIX = "()";

        #endregion
    }
}
