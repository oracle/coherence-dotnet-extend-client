/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;

namespace Tangosol.Util
{
    /// <summary>
    /// This static class contains helper functions for
    /// calculating hash code values for any group of
    /// c# intrinsics.
    /// </summary>
    /// <author>Harvey Raja 2011.08.30</author>
    /// <since>Coherence 3.7.2</since>
    public static class HashHelper
    {
        /// <summary>
        /// Calculate a running hash using the boolean value.
        /// </summary>
        /// <param name="value">
        /// The boolean value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(bool value, int hash)
        {
            // as in Java use two arbitary large prime numbers opposed to the
            // c# default 0 || 1
            return Swizzle(hash) ^ (value ? 1231 : 1237);
        }

        /// <summary>
        /// Calculate a running hash using the byte value.
        /// </summary>
        /// <param name="value">
        /// The byte value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(byte value, int hash)
        {
            return Swizzle(hash) ^ value;
        }

        /// <summary>
        /// Calculate a running hash using the char value.
        /// </summary>
        /// <param name="value">
        /// The char value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(char value, int hash)
        {
            return Swizzle(hash) ^ value;
        }

        /// <summary>
        /// Calculate a running hash using the double value.
        /// </summary>
        /// <param name="value">
        /// The double value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(double value, int hash)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            return Swizzle(hash) ^ (int) (bits ^ (bits >> 32));
        }

        /// <summary>
        /// Calculate a running hash using the float value.
        /// </summary>
        /// <param name="value">
        /// The float value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(float value, int hash)
        {
            return Swizzle(hash) ^ BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        /// <summary>
        /// Calculate a running hash using the int value.
        /// </summary>
        /// <param name="value">
        /// The int value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(int value, int hash)
        {
            return Swizzle(hash) ^ value;
        }

        /// <summary>
        /// Calculate a running hash using the long value.
        /// </summary>
        /// <param name="value">
        /// The long value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(long value, int hash)
        {
            return Swizzle(hash) ^ (int) (value ^ (value >> 32));
        }

        /// <summary>
        /// Calculate a running hash using the short value.
        /// </summary>
        /// <param name="value">
        /// The short value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(short value, int hash)
        {
            return Swizzle(hash) ^ value;
        }

        /// <summary>
        /// Calculate a running hash using the object value.
        /// </summary>
        /// <param name="value">
        /// The object value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(object value, int hash)
        {
            hash = Swizzle(hash);
            if (value == null)
            {
                return hash;
            }
            if (value is bool[])
            {
                return hash ^ Hash((bool[]) value, hash);
            }
            if (value is byte[])
            {
                return hash ^ Hash((byte[]) value, hash);
            }
            if (value is char[])
            {
                return hash ^ Hash((char[]) value, hash);
            }
            if (value is double[])
            {
                return hash ^ Hash((double[]) value, hash);
            }
            if (value is float[])
            {
                return hash ^ Hash((float[]) value, hash);
            }
            if (value is int[])
            {
                return hash ^ Hash((int[]) value, hash);
            }
            if (value is long[])
            {
                return hash ^ Hash((long[]) value, hash);
            }
            if (value is short[])
            {
                return hash ^ Hash((short[]) value, hash);
            }
            if (value is object[])
            {
                return hash ^ Hash((object[]) value, hash);
            }
            if (value is ICollection)
            {
                return hash ^ Hash((ICollection) value, hash);
            }
            return hash ^ value.GetHashCode();
        }

        /// <summary>
        /// Calculate a running hash using the boolean array value.
        /// </summary>
        /// <param name="values">
        /// The boolean array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(bool[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the byte array value.
        /// </summary>
        /// <param name="values">
        /// The byte array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(byte[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the char array value.
        /// </summary>
        /// <param name="values">
        /// The char array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(char[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the double array value.
        /// </summary>
        /// <param name="values">
        /// The double array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(double[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the float array value.
        /// </summary>
        /// <param name="values">
        /// The float array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(float[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the int array value.
        /// </summary>
        /// <param name="values">
        /// The int array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(int[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the long array value.
        /// </summary>
        /// <param name="values">
        /// The long array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(long[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the short array value.
        /// </summary>
        /// <param name="values">
        /// The short array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(short[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the object array value.
        /// </summary>
        /// <param name="values">
        /// The object array value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(object[] values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            int length = values.Length;
            for (int i = 0; i < length; ++i)
            {
                hash = Hash(values[i], hash);
            }
            return hash;
        }

        /// <summary>
        /// Calculate a running hash using the Collection value.  The hash
        /// computed over the Collection's entries is order-independent.
        /// </summary>
        /// <param name="values">
        /// The ICollection value for use in the hash.
        /// </param>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        public static int Hash(ICollection values, int hash)
        {
            hash = Swizzle(hash);
            if (values == null)
            {
                return hash;
            }
            foreach (object value in values)
            {
                hash ^= value.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Shift the running hash value to try and help with
        /// generating unique values given the same input, but
        /// in a different order.
        /// </summary>
        /// <param name="hash">
        /// The running hash value.
        /// </param>
        /// <returns>
        /// The resulting running hash value.
        /// </returns>
        private static int Swizzle(int hash)
        {
            // rotate the current hash value 4 bits to the left
            return (hash << 4) | ((hash >> 28) & 0xF);
        }
    }
}
