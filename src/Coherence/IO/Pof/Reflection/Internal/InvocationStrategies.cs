/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Reflection;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// InvocationStrategies contains two 
    /// <see cref="IInvocationStrategy{T}"/> implementations that abstract 
    /// the underlying mechanisms to retrieve and set a property's value. 
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    public class InvocationStrategies
    {
        #region Inner class: PropertyInvcationStrategy
        
        /// <summary>
        /// A PropertyInvcationStrategy uses a <see cref="PropertyInfo"/> to
        /// dynamically invoke gets and sets on the property.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <typeparam name="T">The containing type.</typeparam>
        /// <since>Coherence 3.7.1</since>
        public class PropertyInvcationStrategy<T> : IInvocationStrategy<T>
                                                    where T : class, new()
        {
            #region Constructors

            /// <summary>
            /// Construct a PropertyInvcationStrategy with the supplied 
            /// PropertyInfo object.
            /// </summary>
            /// <param name="propInfo">
            /// The property that will be used to get and set values.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Iff propInfo is null.
            /// </exception>
            public PropertyInvcationStrategy(PropertyInfo propInfo)
            {
                if (propInfo == null)
                {
                    throw new ArgumentNullException("propInfo", "A non-null property must be supplied to a PropertyInvcationStrategy");
                }
                m_propInfo = propInfo;
            }

            #endregion

            #region IInvocationStrategy members

            /// <summary>
            /// Returns the value of the property.
            /// </summary>
            /// <param name="container">
            /// Container of this and all other properties.
            /// </param>
            /// <returns>
            /// Property value.
            /// </returns>
            public object Get(T container)
            {
                try
                {
                    return m_propInfo.GetValue(container, null);
                }
                catch (Exception e)
                {
                    throw new SystemException("AttributeMetadata accessor is unable to access property " + m_propInfo.Name, e);
                }
            }

            /// <summary>
            /// Sets the parameter value to the property.
            /// </summary>
            /// <param name="container">
            /// Container of this and all other sibling properties.
            /// </param>
            /// <param name="value">
            /// New value to assign to the property.
            /// </param>
            public void Set(T container, object value)
            {
                try
                {
                    m_propInfo.SetValue(container, value, null);
                }
                catch (Exception e)
                {
                    throw new SystemException("AttributeMetadata accessor is unable to set property " + m_propInfo.Name + " to value " + value, e);
                }
            }

            #endregion

            #region Data members

            /**
             * The PropertyInfo this strategy will use to get and set values.
             */
            private readonly PropertyInfo m_propInfo;

            #endregion

        }

        #endregion

        #region Inner class: FieldInvcationStrategy

        /// <summary>
        /// A FieldInvocationStrategy uses a <see cref="FieldInfo"/> to 
        /// dynamically invoke gets and sets on the field.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <typeparam name="T">The containing type.</typeparam>
        /// <since>Coherence 3.7.1</since>
        public class FieldInvcationStrategy<T> : IInvocationStrategy<T> 
                                                    where T : class, new()
        {
            #region Constructors

            /// <summary>
            /// FieldInvocationStrategy must be initialized with an 
            /// appropriate <see cref="FieldInfo"/> object.
            /// </summary>
            /// <param name="fieldInfo">
            /// The fieldInfo that will be used to get and set values.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Iff fieldInfo is null.
            /// </exception>
            public FieldInvcationStrategy(FieldInfo fieldInfo)
            {
                if (fieldInfo == null)
                {
                    throw new ArgumentNullException("fieldInfo", 
                        "A non-null fieldInfo must be supplied to a FieldInvocationStrategy");
                }
                m_fieldInfo = fieldInfo;
            }

            #endregion

            #region IInvocationStrategy implementation

            /// <summary>
            /// Returns the value of the property.
            /// </summary>
            /// <param name="container">
            /// Container of this and all other properties.
            /// </param>
            /// <returns>
            /// Property value.
            /// </returns>
            public virtual object Get(T container)
            {
                try
                {
                    return m_fieldInfo.GetValue(container);
                }
                catch (Exception e)
                {
                    throw new SystemException("AttributeMetadata accessor is unable to access field "
                        + m_fieldInfo.Name, e);
                }
            }

            /// <summary>
            /// Sets the parameter value to the property.
            /// </summary>
            /// <param name="container">
            /// Container of this and all other sibling properties.
            /// </param>
            /// <param name="value">
            /// New value to assign to the property.
            /// </param>
            public virtual void Set(T container, object value)
            {
                try
                {
                    m_fieldInfo.SetValue(container, value);
                }
                catch (Exception e)
                {
                    throw new SystemException("AttributeMetadata accessor is unable to set field "
                        + m_fieldInfo.Name + " to value " + value, e);
                }
            }

            #endregion

            #region Data members

            /**
             * The FieldInfo this strategy will use to get and set values.
             */
            private readonly FieldInfo m_fieldInfo;
            
            #endregion
        }

        #endregion

        #region Inner class: MethodInvocationStrategy

        /// <summary>
        /// A MethodInvocationStrategy uses <see cref="MethodInfo"/>s to 
        /// dynamically invoke getter and setter methods to retrieve and set 
        /// property values.
        /// </summary>
        /// <author>Harvey Raja  2011.07.25</author>
        /// <typeparam name="T">The containing type.</typeparam>
        /// <since>Coherence 3.7.1</since>
        public class MethodInvocationStrategy<T> : IInvocationStrategy<T>
                                                     where T : class, new()
        {
            #region Constructors

            /// <summary>
            /// Based on either the getter or setter derive the missing/
            /// complimenting accessor from the class provided.
            /// </summary>
            /// <param name="method">
            /// Getter or Setter.
            /// </param>
            public MethodInvocationStrategy(MethodInfo method)
            {
                if (method == null)
                {
                    Initialize(null, null);
                }

                MethodInfo compliment = GetCompliment(method);
                bool       isSetter   = method.ReturnType == null || typeof(void).Equals(method.ReturnType);

                Initialize(isSetter ? compliment : method,
                           isSetter ? method : compliment);
            }

            /// <summary>
            /// Construct with the get and set methods.
            /// </summary>
            /// <param name="getter">
            /// <c>T GetX()</c> method
            /// </param>
            /// <param name="setter">
            /// Void <c>>SetX(T a)</c>
            /// </param>
            public MethodInvocationStrategy(MethodInfo getter, MethodInfo setter)
            {
                Initialize(getter, setter);
            }

            #endregion

            #region IInvocationStrategy implementation

            /// <summary>
            /// Returns the value of the property.
            /// </summary>
            /// <param name="container">
            /// Container of this and all other properties.
            /// </param>
            /// <returns>
            /// Property value.
            /// </returns>
            public virtual object Get(T container)
            {
                try
                {
                    return m_getter.Invoke(container, null);
                }
                catch (Exception e)
                {
                    throw new SystemException("AttributeMetadata accessor is unable to access method "
                        + m_getter.Name, e);
                }
            }

            /// <summary>
            /// Sets the parameter value to the property.
            /// </summary>
            /// <param name="container">
            /// Container of this and all other sibling properties.
            /// </param>
            /// <param name="value">
            /// New value to assign to the property.
            /// </param>
            public virtual void Set(T container, object value)
            {
                try
                {
                    m_setter.Invoke(container, new[] { value });
                }
                catch (Exception e)
                {
                    throw new SystemException("AttributeMetadata accessor is unable to call setter "
                        + m_setter.Name + " with value " + value, e);
                }
            }

            #endregion

            #region Helper methods

            /// <summary>
            /// Determine the complement of the provided method in terms of
            /// accessors, i.e. if a set method return the corresponding get 
            /// or is and vice versa.
            /// </summary>
            /// <param name="method">
            /// The method to determine the compliment of.
            /// </param>
            /// <returns>
            /// The method that compliments the method passed.
            /// </returns>
            /// <exception cref="MissingMethodException">
            /// Iff the method could not be found.
            /// </exception>
            /// <exception cref="SystemException">
            /// Iff the compliment method could not be determined by
            /// <see cref="Type.GetMethod(string,System.Type[])"/>.
            /// </exception>
            protected virtual MethodInfo GetCompliment(MethodInfo method)
            {
                if (method == null)
                {
                    return null;
                }

                int accessorType = 0;
                if (method.ReturnType == null || typeof(void).Equals(method.ReturnType)) // setter
                {
                    if (method.GetParameters().Length <= 0)
                    {
                        throw new ArgumentException("Method (" + method + ") should have a parameter");
                    }
                    accessorType = typeof(bool).Equals(method.GetParameters()[0].ParameterType)
                              || typeof(Boolean).Equals(method.GetParameters()[0].ParameterType)
                                  ? 2 : 1;
                }

                string methodName = string.Format("{0}{1}",
                    accessorType == 2 ? "Is" : accessorType == 1 ? "Get" : "Set",
                    method.Name.Substring(method.Name.StartsWith("Is") ? 2 : 3));

                MethodInfo compliment;
                try
                {
                    compliment = accessorType == 0
                       ? method.DeclaringType.GetMethod(methodName, new[] { method.ReturnType })
                       : method.DeclaringType.GetMethod(methodName);
                }
                catch (Exception e)
                {
                    throw new SystemException("An error occurred in discovering the compliment of method = " + method
                        + ", assuming compliment method name is " + methodName, e);
                }

                if (compliment == null)
                {
                    throw new MissingMethodException(method.DeclaringType.Name, methodName);
                }

                return compliment;
            }

            /// <summary>
            /// Initialize ensures both accessors are not null.
            /// </summary>
            /// <param name="getter">
            /// The get accessor.
            /// </param>
            /// <param name="setter">
            /// The set accessor.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Iff getter or setter are null or not accessible.
            /// </exception>
            protected virtual void Initialize(MethodInfo getter, MethodInfo setter)
            {
                if (getter == null || setter == null)
                {
                    throw new ArgumentException("A getter and/or setter could not " +
                        "be determined and is required for a MethodInvocationStrategy");
                }

                m_getter = getter;
                m_setter = setter;
            }

            #endregion

            #region Data members

            /**
             * The MethodInfo this strategy will use to get the value.
             */
            private MethodInfo m_getter;

            /**
             * The MethodInfo this strategy will use to set a value.
             */
            private MethodInfo m_setter;

            #endregion
        }

        #endregion
    }
}
