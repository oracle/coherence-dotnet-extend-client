/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Reflection-based <see cref="IValueExtractor"/> implementation.
    /// </summary>
    /// <author>Cameron Purdy  2002.11.01</author>
    /// <author>Gene Gleyzer  2002.11.01</author>
    /// <author>Ivan Cikic  2006.10.20</author>
    public class ReflectionExtractor : AbstractExtractor, IValueExtractor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Determine the name of the member that this extractor is
        /// configured to invoke.
        /// </summary>
        /// <value>
        /// The name of the member to invoke using reflection.
        /// </value>
        public virtual string MemberName
        {
            get { return m_memberName; }
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
        /// Default constructor.
        /// </summary>
        public ReflectionExtractor()
        {}

        /// <summary>
        /// Construct a <b>ReflectionExtractor</b> based on a member name.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        public ReflectionExtractor(string member)
            : this(member, null, VALUE)
        {
        }

        /// <summary>
        /// Construct a <b>ReflectionExtractor</b>.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="parameters">
        /// The array of arguments to be used in the method invocation;
        /// may be <c>null</c>.
        /// </param>
        public ReflectionExtractor(string member, object[] parameters) 
            : this(member, parameters, VALUE)
        {
        }

        /// <summary>
        /// Construct a <b>ReflectionExtractor</b> based on a method name,
        /// optional parameters and the entry extraction target.
        /// </summary>
        /// <param name="member">
        /// The name of the member to invoke via reflection.
        /// </param>
        /// <param name="parameters">
        /// The array of arguments to be used in the method invocation;
        /// may be <c>null</c>.
        /// </param>
        /// <param name="target">
        /// One of the <see cref="AbstractExtractor.VALUE"/> or
        /// <see cref="AbstractExtractor.KEY"/> values
        /// </param>
        public ReflectionExtractor(string member, object[] parameters, int target)
        {
            Debug.Assert(member != null);
            m_memberName = member;
            m_parameters = parameters;
            m_target = target;
        }

        #endregion

        #region IValueExtractor implementation

        /// <summary>
        /// Extract the value from the passed object.
        /// </summary>
        /// <remarks>
        /// The returned value may be <c>null</c>.
        /// </remarks>
        /// <param name="target">
        /// An object to retrieve the value from.
        /// </param>
        /// <returns>
        /// The extracted value as an object; <c>null</c> is an acceptable
        /// value.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// If this IValueExtractor is incompatible with the passed object to
        /// extract a value from and the implementation <b>requires</b> the
        /// passed object to be of a certain type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this IValueExtractor cannot handle the passed object for any
        /// other reason; an implementor should include a descriptive
        /// message.
        /// </exception>
        public override object Extract(object target)
        {
            if (target == null)
            {
                return null;
            }

            Type   type       = target.GetType();
            string memberName = MemberName;
            try
            {
                MemberInfo member;
                if (type == m_prevType)
                {
                    member = m_prevMember;
                }
                else
                {
                    Object[] parameters = m_parameters;
                    int      paramCount = parameters == null ? 0 : parameters.Length;
                    if (paramCount == 0)
                    {
                        MemberInfo[] members = type.GetMember(memberName);
                        if (members.Length == 0)
                        {
                            throw new ArgumentException("No such member found.");
                        }

                        member       = members[0];
                        m_prevMember = member;
                        m_prevType   = type;
                    }
                    else
                    {
                        Type[] parameterTypes = new Type[paramCount];
                        for (int i = 0; i < paramCount; i++)
                        {
                            parameterTypes[i] = parameters[i].GetType();
                        }

                        member = type.GetMethod(memberName, parameterTypes);
                    }
                }

                if (member.MemberType == MemberTypes.Method)
                {
                    return ((MethodInfo) member).Invoke(target, m_parameters);
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    return ((PropertyInfo) member).GetValue(target, null);
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    return ((FieldInfo) member).GetValue(target);
                }
                else
                {
                    throw new ArgumentException("Specified member is neither property nor a method.");
                }
            }
            catch (NullReferenceException)
            {
                throw new Exception(SuggestExtractFailureCause(type, memberName));
            }
            catch (Exception e)
            {
                throw new Exception(type.Name + '.' + memberName + '(' + target + ')', e);
            }
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <see cref="IValueExtractor"/> with another object to
        /// determine equality.
        /// </summary>
        /// <remarks>
        ///  Two <b>IValueExtractor</b> objects, <i>ve1</i> and <i>ve2</i>
        /// are considered equal iff <tt>ve1.Extract(o)</tt> equals
        /// <tt>ve2.Extract(o)</tt> for all values of <tt>o</tt>.
        /// </remarks>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <see cref="IValueExtractor"/> and the passed
        /// object are quivalent <b>IValueExtractor</b>s.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ReflectionExtractor)
            {
                ReflectionExtractor that = (ReflectionExtractor) o;
                return m_memberName.Equals(that.m_memberName) &&
                       CollectionUtils.EqualsDeep(this.m_parameters, that.m_parameters);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <see cref="IValueExtractor"/>
        /// object according to the general <b>object.GetHashCode</b>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>IValueExtractor</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_memberName.GetHashCode();
        }

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
            sb.Append('.').Append(m_memberName).Append('(');
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

        #region Helper methods

        /// <summary>
        /// Return a message suggesting a possible cause for a failure to
        /// extract a value.
        /// </summary>
        /// <param name="targetType">
        /// The target object's type.
        /// </param>
        /// <param name="memberName">
        /// The method name.
        /// </param>
        /// <returns>
        /// A message suggesting a possible cause for a failure.
        /// </returns>
        private string SuggestExtractFailureCause(Type targetType, string memberName)
        {
            string msg = "Missing or inaccessible member: " +
                          targetType.Name + '.' + memberName;

            if (typeof(CacheEventArgs).IsAssignableFrom(targetType))
            {
                msg += " (the object is a CacheEventArgs, which may "
                        + "suggest that a raw IFilter is "
                        + "being used to filter cache events rather than a "
                        + "CacheEventFilter)";
            }

            return msg;
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
            m_memberName = reader.ReadString(0);
            
            int paramCount = reader.ReadInt32(1);
            // slot #1 is taken by pre-Coherence 3.5 versions for the number
            // of arguments in the parameter array
            if (paramCount > 0)
            {
                // fully backwards compatible implementation
                Object[] parameters = new Object[paramCount];
                for (int i = 0; i < paramCount; i++)
                {
                    parameters[i] = reader.ReadObject(i + 2);
                }
                m_parameters = parameters;    
            } 
            else
            {
                // slot #2 is used since Coherence 3.5 to store the entirety
                // of the arguments (as opposed to the first of the arguments)
                m_parameters = (object[]) reader.ReadArray(2);
                m_target     = reader.ReadInt32(3);
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
        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, m_memberName);
            // slot #1 is not used since Coherence 3.5
            writer.WriteArray(2, m_parameters);
            writer.WriteInt32(3, m_target);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The name of the member to invoke.
        /// </summary>
        protected string m_memberName;

        /// <summary>
        /// The parameter array.
        /// </summary>
        protected Object[] m_parameters;

        /// <summary>
        /// The type of the object that the cached reflection member is
        /// from.
        /// </summary>
        [NonSerialized]
        private Type m_prevType;

        /// <summary>
        /// A cached reflection member (to avoid repeated look-ups).
        /// </summary>
        [NonSerialized]
        private MemberInfo m_prevMember;

        #endregion
    }
}