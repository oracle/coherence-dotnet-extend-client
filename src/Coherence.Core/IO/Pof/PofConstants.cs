/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Globalization;

using Tangosol.Util;

namespace Tangosol.IO.Pof
{
    /// <summary>
    /// The constants related to POF streams.
    /// </summary>
    /// <author>Cameron Purdy, Jason Howes  2006.07.11</author>
    /// <author>Aleksandar Seovic  2006.08.08</author>
    /// <since>Coherence 3.2</since>
    internal struct PofConstants
    {
        //
        // POF intrinsic type constants. The hex value to the right is the packed
        // integer value of the constant.
        //
        internal const int T_INT16                = - 1; // 0x40;
        internal const int T_INT32                = - 2; // 0x41;
        internal const int T_INT64                = - 3; // 0x42;
        internal const int T_INT128               = - 4; // 0x43;
        internal const int T_FLOAT32              = - 5; // 0x44;
        internal const int T_FLOAT64              = - 6; // 0x45;
        internal const int T_FLOAT128             = - 7; // 0x46;
        internal const int T_DECIMAL32            = - 8; // 0x47;
        internal const int T_DECIMAL64            = - 9; // 0x48;
        internal const int T_DECIMAL128           = - 10; // 0x49;
        internal const int T_BOOLEAN              = - 11; // 0x4A;
        internal const int T_OCTET                = - 12; // 0x4B;
        internal const int T_OCTET_STRING         = - 13; // 0x4C;
        internal const int T_CHAR                 = - 14; // 0x4D;
        internal const int T_CHAR_STRING          = - 15; // 0x4E;
        internal const int T_DATE                 = - 16; // 0x4F;
        internal const int T_YEAR_MONTH_INTERVAL  = - 17; // 0x50;
        internal const int T_TIME                 = - 18; // 0x51;
        internal const int T_TIME_INTERVAL        = - 19; // 0x52;
        internal const int T_DATETIME             = - 20; // 0x53;
        internal const int T_DAY_TIME_INTERVAL    = - 21; // 0x54;
        internal const int T_COLLECTION           = - 22; // 0x55;
        internal const int T_UNIFORM_COLLECTION   = - 23; // 0x56;
        internal const int T_ARRAY                = - 24; // 0x57;
        internal const int T_UNIFORM_ARRAY        = - 25; // 0x58;
        internal const int T_SPARSE_ARRAY         = - 26; // 0x59;
        internal const int T_UNIFORM_SPARSE_ARRAY = - 27; // 0x5A;
        internal const int T_MAP                  = - 28; // 0x5B;
        internal const int T_UNIFORM_KEYS_MAP     = - 29; // 0x5C;
        internal const int T_UNIFORM_MAP          = - 30; // 0x5D;
        internal const int T_IDENTITY             = - 31; // 0x5E;
        internal const int T_REFERENCE            = - 32; // 0x5F;

        //
        // POF compact "small" values. The hex value to the right is the packed
        // integer value of the value.
        //
        internal const int V_BOOLEAN_FALSE      = - 33; // 0x60;
        internal const int V_BOOLEAN_TRUE       = - 34; // 0x61;
        internal const int V_STRING_ZERO_LENGTH = - 35; // 0x62;
        internal const int V_COLLECTION_EMPTY   = - 36; // 0x63;
        internal const int V_REFERENCE_NULL     = - 37; // 0x64;
        internal const int V_FP_POS_INFINITY    = - 38; // 0x65;
        internal const int V_FP_NEG_INFINITY    = - 39; // 0x66;
        internal const int V_FP_NAN             = - 40; // 0x67;
        internal const int V_INT_NEG_1          = - 41; // 0x68;
        internal const int V_INT_0              = - 42; // 0x69;
        internal const int V_INT_1              = - 43; // 0x6A;
        internal const int V_INT_2              = - 44; // 0x6B;
        internal const int V_INT_3              = - 45; // 0x6C;
        internal const int V_INT_4              = - 46; // 0x6D;
        internal const int V_INT_5              = - 47; // 0x6E;
        internal const int V_INT_6              = - 48; // 0x6F;
        internal const int V_INT_7              = - 49; // 0x70;
        internal const int V_INT_8              = - 50; // 0x71;
        internal const int V_INT_9              = - 51; // 0x72;
        internal const int V_INT_10             = - 52; // 0x73;
        internal const int V_INT_11             = - 53; // 0x74;
        internal const int V_INT_12             = - 54; // 0x75;
        internal const int V_INT_13             = - 55; // 0x76;
        internal const int V_INT_14             = - 56; // 0x77;
        internal const int V_INT_15             = - 57; // 0x78;
        internal const int V_INT_16             = - 58; // 0x79;
        internal const int V_INT_17             = - 59; // 0x7A;
        internal const int V_INT_18             = - 60; // 0x7B;
        internal const int V_INT_19             = - 61; // 0x7C;
        internal const int V_INT_20             = - 62; // 0x7D;
        internal const int V_INT_21             = - 63; // 0x7E;
        internal const int V_INT_22             = - 64; // 0x7F;

        //
        // POF constant indicating an unknown type. Not a type written to the
        // stream.
        //
        internal const int T_UNKNOWN            = -65; // 0x1C0;

        //
        // Constants representing .NET Object types.
        //
        internal const int N_NULL                = 0;
        internal const int N_BOOLEAN             = 1;
        internal const int N_BYTE                = 2;
        internal const int N_CHARACTER           = 3;
        internal const int N_INT16               = 4;
        internal const int N_INT32               = 5;
        internal const int N_INT64               = 6;
        internal const int N_INT128              = 7;
        internal const int N_SINGLE              = 8;
        internal const int N_DOUBLE              = 9;
      //internal const int N_QUAD                = 10;
        internal const int N_DECIMAL             = 11;
        internal const int N_BINARY              = 12;
        internal const int N_STRING              = 13;
        internal const int N_DATE                = 14;
        internal const int N_TIME                = 15;
        internal const int N_DATETIME            = 16;
      //internal const int J_TIMESTAMP           = 17;
      //internal const int N_RAW_DATE            = 18;
      //internal const int N_RAW_TIME            = 19;
      //internal const int J_RAW_DATETIME        = 20;
        internal const int N_YEAR_MONTH_INTERVAL = 21;
        internal const int N_TIME_INTERVAL       = 22;
        internal const int N_DAY_TIME_INTERVAL   = 23;
        internal const int N_BOOLEAN_ARRAY       = 24;
        internal const int N_BYTE_ARRAY          = 25;
        internal const int N_CHAR_ARRAY          = 26;
        internal const int N_INT16_ARRAY         = 27;
        internal const int N_INT32_ARRAY         = 28;
        internal const int N_INT64_ARRAY         = 29;
        internal const int N_SINGLE_ARRAY        = 30;
        internal const int N_DOUBLE_ARRAY        = 31;
        internal const int N_OBJECT_ARRAY        = 32;
        internal const int N_SPARSE_ARRAY        = 33;
        internal const int N_COLLECTION          = 34;
        internal const int N_DICTIONARY          = 35;
        internal const int N_USER_TYPE           = 36;

        /// <summary>
        /// Maximum scale for the IEEE-754r 32-bit decimal format.
        /// </summary>
        internal const int MAX_DECIMAL32_SCALE = 96;

        /// <summary>
        /// Minimum scale for the IEEE-754r 32-bit decimal format.
        /// </summary>
        public static readonly int MIN_DECIMAL32_SCALE;

        /// <summary>
        /// Maximum unscaled value for the IEEE-754r 32-bit decimal format.
        /// </summary>
        public static readonly Decimal MAX_DECIMAL32_UNSCALED;

        /// <summary>
        /// Maximum scale for the IEEE-754r 64-bit decimal format.
        /// </summary>
        internal const int MAX_DECIMAL64_SCALE = 384;

        /// <summary>
        /// Minimum scale for the IEEE-754r 64-bit decimal format.
        /// </summary>
        public static readonly int MIN_DECIMAL64_SCALE;

        /// <summary>
        /// Maximum unscaled value for the IEEE-754r 64-bit decimal format.
        /// </summary>
        public static readonly Decimal MAX_DECIMAL64_UNSCALED;

        /// <summary>
        /// Maximum scale for the IEEE-754r 128-bit decimal format.
        /// </summary>
        internal const int MAX_DECIMAL128_SCALE = 6144;

        /// <summary>
        /// Minimum scale for the IEEE-754r 128-bit decimal format.
        /// </summary>
        public static readonly int MIN_DECIMAL128_SCALE;

        /// <summary>
        /// Maximum unscaled value for the IEEE-754r 128-bit decimal format.
        /// </summary>
        public static readonly Decimal MAX_DECIMAL128_UNSCALED;

        /// <summary>
        /// Maximum scale for the .NET 96-bit decimal format.
        /// </summary>
        internal const int MAX_DECIMAL_SCALE = 28;

        /// <summary>
        /// Minimum scale for the .NET 96-bit decimal format.
        /// </summary>
        public static readonly int MIN_DECIMAL_SCALE = 0;

        /// <summary>
        /// Maximum unscaled value for the Decimal 96-bit decimal format.
        /// </summary>
        public static readonly Decimal MAX_DECIMAL_UNSCALED;

        // CLOVER:OFF

        static PofConstants()
        {
            MIN_DECIMAL32_SCALE     = 1 - MAX_DECIMAL32_SCALE;
            MAX_DECIMAL32_UNSCALED  = Decimal.Parse(StringUtils.Dup('9', 7), NumberStyles.Any);
            MIN_DECIMAL64_SCALE     = 1 - MAX_DECIMAL64_SCALE;
            MAX_DECIMAL64_UNSCALED  = Decimal.Parse(StringUtils.Dup('9', 16), NumberStyles.Any);
            MIN_DECIMAL128_SCALE    = 1 - MAX_DECIMAL128_SCALE;
            // the following value can't be instantiated, no data type big enough
            // these constants are instantiated when referenced, and this failure
            // prevents instantiation of the other variables.
            // MAX_DECIMAL128_UNSCALED = Decimal.Parse(StringUtils.Dup('9', 34), NumberStyles.Any);
            MAX_DECIMAL128_UNSCALED = 0;
            MAX_DECIMAL_UNSCALED    = Decimal.Parse(StringUtils.Dup('9', 28), NumberStyles.Any);
        }

        // CLOVER:ON
    }
}