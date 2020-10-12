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

using Tangosol.IO.Pof;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Reflection-based <see cref="IValueUpdater"/> implementation.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.27</author>
    /// <author>Ivan Cikic  2006.10.20</author>
    public class ReflectionUpdater : IValueUpdater, IPortableObject
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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReflectionUpdater()
        {}

        /// <summary>
        /// Construct a <b>ReflectionUpdater</b> for a given method name.
        /// </summary>
        /// <remarks>
        /// This implementation assumes that the corresponding types will
        /// have one and only one member with a specified name and, in the
        /// case this member is method, it will have exactly one parameter.
        /// </remarks>
        /// <param name="memberName">
        /// The name of the method to invoke via reflection.
        /// </param>
        public ReflectionUpdater(string memberName)
        {
            Debug.Assert(memberName != null);
            m_memberName = memberName;
        }

        #endregion

        #region IValueUpdater implementation

        /// <summary>
        /// Update the state of the passed target object using the passed
        /// value.
        /// </summary>
        /// <param name="target">
        /// The object to update the state of.
        /// </param>
        /// <param name="value">
        /// The new value to update the state with.
        /// </param>
        /// <exception cref="InvalidCastException">
        /// If this IValueUpdater is incompatible with the passed target
        /// object or the value and the implementation <b>requires</b> the
        /// passed object or the value to be of a certain type.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this IValueUpdater cannot handle the passed target object or
        /// value for any other reason; an implementor should include a
        /// descriptive message.
        /// </exception>
        public virtual void Update(object target, object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException("Target object is missing for the Updater: " + this);
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
                    MemberInfo[] members = type.GetMember(memberName);

                    if (members.Length == 0)
                    {
                        throw new ArgumentException("No such member found.");
                    }

                    member       = members[0];
                    m_prevMember = member;
                    m_prevType   = type;
                }

                if (member.MemberType == MemberTypes.Method)
                {
                    ((MethodInfo) member).Invoke(target, new object[] { value });
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    ((PropertyInfo) member).SetValue(target, value, null);
                }
                else if (member.MemberType == MemberTypes.Field)
                {
                    ((FieldInfo) member).SetValue(target, value);
                }
                else
                {
                    throw new ArgumentException("Specified member is neither property nor a method.");
                }
            }
            catch (NullReferenceException)
            {
                throw new Exception("Missing or inaccessible member: " + type.Name + '.' + memberName);
            }
            catch (Exception e)
            {
                throw new Exception(type.Name + '.' + memberName + '(' + target + ')', e);
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
        public virtual void ReadExternal(IPofReader reader)
        {
            m_memberName = reader.ReadString(0);
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
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <see cref="IValueUpdater"/> with another object to
        /// determine equality.
        /// </summary>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <see cref="IValueUpdater"/> and the passed
        /// object are quivalent <b>IValueUpdater</b>s.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ReflectionUpdater)
            {
                ReflectionUpdater that = (ReflectionUpdater) o;
                return m_memberName.Equals(that.m_memberName);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the <see cref="IValueUpdater"/>
        /// object according to the general <b>object.GetHashCode</b>
        /// contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>IValueUpdater</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return m_memberName.GetHashCode();
        }

        /// <summary>
        /// Provide a human-readable description of this
        /// <see cref="IValueUpdater"/> object.
        /// </summary>
        /// <returns>
        /// A human-readable description of this <b>IValueUpdater</b>
        /// object.
        /// </returns>
        public override string ToString()
        {
            return '&' + m_memberName;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The name of the member to invoke.
        /// </summary>
        protected string m_memberName;

        /// <summary>
        /// The class of the object that the cached reflection method is
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