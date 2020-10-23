/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Reflection;
using System.Text;

namespace Tangosol.Util
{
    /// <summary>
    /// Miscellaneuos utility methods for object manipulation.
    /// </summary>
    /// <author>Aleksandar Seovic  2007.07.31</author>
    public abstract class ObjectUtils
    {
        /// <summary>
        /// Creates object instance using constructor that matches
        /// specified parameters.
        /// </summary>
        /// <param name="objectType">
        /// Type of object to create.
        /// </param>
        /// <param name="parameters">
        /// Serializer parameters.
        /// </param>
        /// <returns>
        /// An instance of the specified <paramref name="objectType"/>.
        /// </returns>
        public static object CreateInstance(Type objectType, params object[] parameters)
        {
            Type[] paramTypes  = GetTypes(parameters);
            ConstructorInfo ci = objectType.GetConstructor(paramTypes);

            if (ci == null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Could not find a constructor for ").Append(objectType.FullName).Append("(");
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(paramTypes[i] == null ? "null" : paramTypes[i].FullName);
                }
                sb.Append(")");

                throw new ArgumentException(sb.ToString());
            }

            return ci.Invoke(parameters);
        }

        /// <summary>
        /// Creates object instance using constructor that matches
        /// specified parameters.
        /// </summary>
        /// <remarks>
        /// Returns null rather than throwing an exception, if the 
        /// specified constructor doesn't exist or fails to be invoked.
        /// </remarks>
        /// <param name="objectType">
        /// Type of object to create.
        /// </param>
        /// <param name="parameters">
        /// Serializer parameters.
        /// </param>
        /// <returns>
        /// An instance of the specified <paramref name="objectType"/>.
        /// </returns>
        public static object CreateInstanceSafe(Type objectType, params object[] parameters)
        {
            object o = null;

            ConstructorInfo ci = objectType.GetConstructor(GetTypes(parameters));
            if (ci != null)
            {
                try 
                {
                    o = ci.Invoke(parameters);
                }
                catch (Exception)
                {}
            }

            return o;
        }

        /// <summary>
        /// Return <c>true</c> if the specified object is immutable, 
        /// <c>false</c> otherwise.
        /// </summary>
        /// <param name="obj">Object to check for immutability.</param>
        /// <returns>
        /// <c>true</c> if the specified object is immutable, 
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool IsImmutable(object obj)
        {
            return obj == null
                   || Array.IndexOf(IMMUTABLE_TYPES, obj.GetType()) >= 0;
        }

        /// <summary>
        /// Return an array of types of specified objects.
        /// </summary>
        /// <param name="parameters">
        /// paramters
        /// </param>
        /// <returns>
        /// An array of types
        /// </returns>
        private static Type[] GetTypes(params object[] parameters)
        {
            Type[] paramTypes = parameters == null ? new Type[0] : new Type[parameters.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = parameters[i].GetType();
            }
            
            return paramTypes;
        }

        /// <summary>
        /// An array of BCL immutable types.
        /// </summary>
        private static readonly Type[] IMMUTABLE_TYPES = new Type[] {
            typeof(String), typeof(Boolean), typeof(DateTime), typeof(TimeSpan),
            typeof(Int16),  typeof(Int32),   typeof(Int64), 
            typeof(UInt16), typeof(UInt32),  typeof(UInt64),
            typeof(Single), typeof(Double),  typeof(Decimal),
            typeof(Byte),   typeof(SByte),   typeof(Char),    
            typeof(Guid),   typeof(IntPtr),  typeof(UIntPtr)                                            
        };

        /// <summary>
        /// Constant that allows one to differentiate between a non-existent
        /// value and <c>null</c>.
        /// </summary>
        public static readonly object NO_VALUE = new object();
    }
}