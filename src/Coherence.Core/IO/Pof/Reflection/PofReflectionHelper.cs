/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;

using Tangosol.Util;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// Collection of helper methods for POF reflection.
    /// </summary>
    /// <author>David Guy  2009.09.14</author>
    /// <author>Ana Cikic  2009.09.25</author>
    /// <since>Coherence 3.5.2</since>
    public class PofReflectionHelper
    {
        /// <summary>
        /// Determine the type associated with the given type identifier.
        /// </summary>
        /// <param name="typeId">
        /// The Pof type identifier; includes Pof intrinsics, Pof compact
        /// values, and user types.
        /// </param>
        /// <param name="ctx">
        /// The <see cref="IPofContext"/>.
        /// </param>
        /// <returns>
        /// The type associated with the specified type identifier or
        /// <c>null</c> for types with no mapping.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified type is a user type that is unknown to this
        /// <b>IPofContext</b>.
        /// </exception>
        public static Type GetType(int typeId, IPofContext ctx)
        {
            if (typeId >= 0)
            {
                return ctx.GetType(typeId);
            }

            switch (typeId)
            {
                case PofConstants.T_INT16:
                    return typeof(short);

                case PofConstants.T_INT32:
                case PofConstants.V_INT_NEG_1:
                case PofConstants.V_INT_0:
                case PofConstants.V_INT_1:
                case PofConstants.V_INT_2:
                case PofConstants.V_INT_3:
                case PofConstants.V_INT_4:
                case PofConstants.V_INT_5:
                case PofConstants.V_INT_6:
                case PofConstants.V_INT_7:
                case PofConstants.V_INT_8:
                case PofConstants.V_INT_9:
                case PofConstants.V_INT_10:
                case PofConstants.V_INT_11:
                case PofConstants.V_INT_12:
                case PofConstants.V_INT_13:
                case PofConstants.V_INT_14:
                case PofConstants.V_INT_15:
                case PofConstants.V_INT_16:
                case PofConstants.V_INT_17:
                case PofConstants.V_INT_18:
                case PofConstants.V_INT_19:
                case PofConstants.V_INT_20:
                case PofConstants.V_INT_21:
                case PofConstants.V_INT_22:
                    return typeof(int);

                case PofConstants.T_INT64:
                    return typeof(long);

                case PofConstants.T_INT128:
                    throw new NotSupportedException("T_INT128 type is not supported.");

                case PofConstants.T_FLOAT32:
                    return typeof(float);

                case PofConstants.T_FLOAT64:
                    return typeof(double);

                case PofConstants.T_FLOAT128:
                    throw new NotSupportedException("T_FLOAT128 type is not supported.");

                case PofConstants.V_FP_POS_INFINITY:
                    return typeof(double);

                case PofConstants.V_FP_NEG_INFINITY:
                    return typeof(double);

                case PofConstants.V_FP_NAN:
                    return typeof(double);

                case PofConstants.T_DECIMAL32:
                    return typeof(decimal);

                case PofConstants.T_DECIMAL64:
                    throw new NotSupportedException("T_DECIMAL64 type is not supported.");

                case PofConstants.T_DECIMAL128:
                    throw new NotSupportedException("T_DECIMAL128 type is not supported.");

                case PofConstants.T_BOOLEAN:
                case PofConstants.V_BOOLEAN_FALSE:
                case PofConstants.V_BOOLEAN_TRUE:
                    return typeof(bool);

                case PofConstants.T_OCTET:
                    return typeof(byte);

                case PofConstants.T_OCTET_STRING:
                    return typeof(Binary);

                case PofConstants.T_CHAR:
                    return typeof(char);

                case PofConstants.T_CHAR_STRING:
                case PofConstants.V_STRING_ZERO_LENGTH:
                    return typeof(string);

                case PofConstants.T_DATE:
                    return typeof(DateTime);

                case PofConstants.T_TIME:
                    return typeof(RawTime);

                case PofConstants.T_DATETIME:
                    return typeof(DateTime);

                case PofConstants.T_YEAR_MONTH_INTERVAL:
                    return typeof(RawYearMonthInterval);

                case PofConstants.T_TIME_INTERVAL:
                case PofConstants.T_DAY_TIME_INTERVAL:
                    return typeof(TimeSpan);

                case PofConstants.T_COLLECTION:
                case PofConstants.T_UNIFORM_COLLECTION:
                case PofConstants.V_COLLECTION_EMPTY:
                    return typeof(ICollection);

                case PofConstants.T_MAP:
                case PofConstants.T_UNIFORM_KEYS_MAP:
                case PofConstants.T_UNIFORM_MAP:
                    return typeof(IDictionary);

                case PofConstants.T_SPARSE_ARRAY:
                    return typeof(ILongArray);

                case PofConstants.T_ARRAY:
                    return typeof(object[]);

                case PofConstants.T_UNIFORM_ARRAY:
                case PofConstants.T_UNIFORM_SPARSE_ARRAY:
                case PofConstants.V_REFERENCE_NULL:
                    // ambiguous - could be either an array or SparseArray
                    return null;

                case PofConstants.T_IDENTITY:
                case PofConstants.T_REFERENCE:
                    throw new ArgumentException(typeId + " has no " +
                        "mapping to a class");

                default:
                    throw new ArgumentException(typeId + " is an " +
                        "invalid type");
            }
        }

        /// <summary>
        /// Validate that the supplied object is compatible with the
        /// specified type.
        /// </summary>
        /// <param name="o">
        /// The object.
        /// </param>
        /// <param name="typeId">
        /// The Pof type identifier; includes Pof intrinsics, Pof compact
        /// values, and user types.
        /// </param>
        /// <param name="ctx">
        /// The <see cref="IPofContext"/>.
        /// </param>
        /// <returns>
        /// The original object.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If the specified type is a user type that is unknown to this
        /// <b>IPofContext</b> or there is no type mapping.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// If the specified object is not assignable to the specified type.
        /// </exception>
        public static object EnsureType(object o, int typeId, IPofContext ctx)
        {
            Type type = GetType(typeId, ctx);
            if (type == null)
            {
                throw new ArgumentException("Unknown or ambiguous type: " + typeId);
            }
            if (!type.IsAssignableFrom(o.GetType()))
            {
                throw new InvalidCastException(o.GetType().FullName +
                    " is not assignable to "+ type.FullName);
            }
            return o;
        }
    }
}
