/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;

using Tangosol.IO.Pof;
using Tangosol.Util.Comparator;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// A synthetic <see cref="IValueExtractor"/> that returns a result of
    /// comparison between two values extracted from the same target.
    /// </summary>
    /// <remarks>
    /// <p>
    /// In a most general case, the extracted value represents an integer
    /// value calculated accordingly to the contract of 
    /// <see cref="IComparable.CompareTo"/> or
    /// <see cref="IComparer.Compare"/> methods. However, in more specific
    /// cases, when the compared values are of common numeric type, the
    /// <b>ComparisonValueExtractor</b> will return a numeric difference
    /// between those values. The .NET type of the comparing values will
    /// dictate the .NET type of the result.</p>
    /// <p>
    /// For example, lets assume that a cache contains business objects that
    /// have two properties: SellPrice and BuyPrice (both double). Then, to
    /// query for all objects that have SellPrice less than BuyPrice we would
    /// use the following:</p>
    /// <code>
    /// ValueExtractor extractDiff = new ComparisonValueExtractor(
    ///   new ReflectionExtractor("SellPrice"),
    ///   new ReflectionExtractor("BuyPrice"));
    /// Filter filter = new LessFilter(extractDiff, 0.0);
    /// ICollection entries = cache.GetEntries(filter);
    /// </code>
    /// </remarks>
    /// <author>Gene Gleyzer  2008.02.15</author>
    /// <author>Ana Cikic  2008.04.04</author>
    /// <since>Coherence 3.4</since>
    public class ComparisonValueExtractor : AbstractCompositeExtractor
    {
        #region Properties

        /// <summary>
        /// Return an <see cref="IComparer"/> used by this extractor.
        /// </summary>
        /// <value>
        /// An <b>IComparer</b> used by this extractor; <c>null</c> if the
        /// natural value comparison should be used.
        /// </value>
        public virtual IComparer Comparer
        {
            get { return m_comparer; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ComparisonValueExtractor()
        {}

        /// <summary>
        /// Construct a <b>ComparisonValueExtractor</b> based on a specified
        /// <see cref="IValueExtractor"/> array.
        /// </summary>
        /// <param name="extractors">
        /// The <see cref="IValueExtractor"/> array.
        /// </param>
        public ComparisonValueExtractor(IValueExtractor[] extractors) : base(extractors)
        {}

        /// <summary>
        /// Construct a <b>ComparisonValueExtractor</b> based on two member
        /// names.
        /// </summary>
        /// <remarks>
        /// Note: values returned by both methods must be
        /// <see cref="IComparable"/>.
        /// </remarks>
        /// <param name="member1">
        /// The name of the first member to invoke via reflection.
        /// </param>
        /// <param name="member2">
        /// The name of the second member to invoke via reflection.
        /// </param>
        public ComparisonValueExtractor(string member1, string member2) : this(member1, member2, null)
        {}

        /// <summary>
        /// Construct a <b>ComparisonValueExtractor</b> based on two method
        /// names and a <see cref="IComparer"/> object.
        /// </summary>
        /// <param name="member1">
        /// The name of the first member to invoke via reflection.
        /// </param>
        /// <param name="member2">
        /// The name of the second member to invoke via reflection.
        /// </param>
        /// <param name="comp">
        /// The comparer used to compare the extracted values (optional).
        /// </param>
        public ComparisonValueExtractor(string member1, string member2, IComparer comp)
            : this(new ReflectionExtractor(member1), new ReflectionExtractor(member2), comp)
        {}

        /// <summary>
        /// Construct a <b>ComparisonValueExtractor</b> based on two specified
        /// extractors.
        /// </summary>
        /// <remarks>
        /// Note: values returned by both extractors must be
        /// <see cref="IComparable"/>.
        /// </remarks>
        /// <param name="ve1">
        /// The <see cref="IValueExtractor"/> for the first value.
        /// </param>
        /// <param name="ve2">
        /// The <see cref="IValueExtractor"/> for the second value.
        /// </param>
        public ComparisonValueExtractor(IValueExtractor ve1, IValueExtractor ve2) : this(ve1, ve2, null)
        {}

        /// <summary>
        /// Construct a <b>ComparisonValueExtractor</b> based on two specified
        /// extractors and a <see cref="IComparer"/> object.
        /// </summary>
        /// <param name="ve1">
        /// The <see cref="IValueExtractor"/> for the first value.
        /// </param>
        /// <param name="ve2">
        /// The <see cref="IValueExtractor"/> for the second value.
        /// </param>
        /// <param name="comp">
        /// The comparer used to compare the extracted values (optional).
        /// </param>
        public ComparisonValueExtractor(IValueExtractor ve1, IValueExtractor ve2, IComparer comp)
            : base (new IValueExtractor[] {ve1, ve2})
        {
            m_comparer = comp;
        }

        #endregion

        #region IValueExtractor implementation

        /// <summary>
        /// Extract the value from the passed object.
        /// </summary>
        /// <remarks>
        /// The returned value may be <c>null</c>.
        /// </remarks>
        /// <param name="obj">
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
        /// If this AbstractExtractor cannot handle the passed object for any
        /// other reason; an implementor should include a descriptive
        /// message.
        /// </exception>
        public override object Extract(object obj)
        {
            IValueExtractor[] extractors = Extractors;
            IComparer         comparer   = Comparer;

            object o1 = extractors[0].Extract(obj);
            object o2 = extractors[1].Extract(obj);

            if (NumberUtils.IsNumber(o1) && NumberUtils.IsNumber(o2) && comparer == null)
            {
                StreamFormat type;

                if (o1.GetType() == o2.GetType())
                {
                    // most common case; same types
                    type = GetStreamFormat(o1);
                }
                else
                {
                    StreamFormat[] types = new StreamFormat[] {
                            StreamFormat.Byte,      // 0
                            StreamFormat.Short,     // 1
                            StreamFormat.Int,       // 2
                            StreamFormat.Long,      // 3
                            StreamFormat.Float,     // 4
                            StreamFormat.Double,    // 5
                            StreamFormat.RawInt128, // 6
                            StreamFormat.Decimal    // 7
                        };

                    StreamFormat type1 = GetStreamFormat(o1);
                    StreamFormat type2 = GetStreamFormat(o2);
                    int typesCount, ix1, ix2;

                    ix1 = ix2 = typesCount = types.Length;
                    for (int i = 0; i < typesCount; i++)
                    {
                        StreamFormat t = types[i];
                        if (t == type1)
                        {
                            ix1 = i;
                        }
                        if (t == type2)
                        {
                            ix2 = i;
                        }
                    }

                    switch (Math.Max(ix1, ix2))
                    {
                        case 0:
                        case 1:
                        case 2:
                            type = StreamFormat.Int;
                            break;
                        case 3:
                            type = StreamFormat.Long;
                            break;
                        case 4:
                        case 5:
                            type = StreamFormat.Double;
                            break;
                        case 6:
                        case 7:
                            type = StreamFormat.Decimal;
                            o1 = EnsureDecimal(o1);
                            o2 = EnsureDecimal(o2);
                            break;
                        default:
                            type = StreamFormat.None;
                            break;
                    }
                }

                switch (type)
                {
                    case StreamFormat.Byte:
                    case StreamFormat.Short:
                    case StreamFormat.Int:
                        return Convert.ToInt32(o1) - Convert.ToInt32(o2);

                    case StreamFormat.Long:
                        return Convert.ToInt64(o1) - Convert.ToInt64(o2);

                    case StreamFormat.Float:
                        return Convert.ToSingle(o1) - Convert.ToSingle(o2);

                    case StreamFormat.Double:
                        return Convert.ToDouble(o1) - Convert.ToDouble(o2);
                    
                    case StreamFormat.RawInt128:
                        return NumberUtils.DecimalToRawInt128(Decimal.Subtract(((RawInt128) o1).ToDecimal(), ((RawInt128) o2).ToDecimal()));
 
                    case StreamFormat.Decimal:
                        return Decimal.Subtract(Convert.ToDecimal(o1), Convert.ToDecimal(o2));
                }
            }
            return SafeComparer.CompareSafe(comparer, o1, o2);
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
            
            m_comparer = (IComparer) reader.ReadObject(1);
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

            writer.WriteObject(1, m_comparer);
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Turn the specified number into a Decimal.
        /// </summary>
        /// <param name="num">
        /// Number (byte, short, int, long, float, double, or Decimal).
        /// </param>
        /// <returns>
        /// Specified number turned into a Decimal.
        /// </returns>
        private static Decimal EnsureDecimal(object num)
        {
            if (num is Decimal)
            {
                return (Decimal) num;
            }
            else if (num is int || num is byte || num is short)
            {
                return new Decimal((int) num);
            }
            else if (num is long)
            {
                return new Decimal((long) num);
            }
            else if (num is float)
            {
                return new Decimal((float) num);
            }
            else if (num is double)
            {
                return new Decimal((double) num);
            }
            else if (num is RawInt128)
            {
                return ((RawInt128) num).ToDecimal();
            }
            else
            {
                throw new ArgumentException("not a number");
            }
        }

        /// <summary>
        /// Select an optimal stream format to use to store the passed object
        /// in a stream.
        /// </summary>
        /// <param name="o">
        /// An object.
        /// </param>
        /// <returns>
        /// A stream format to use to store the object in a stream.
        /// </returns>
        protected static StreamFormat GetStreamFormat(object o)
        {
            return o == null      ? StreamFormat.None
                 : o is int       ? StreamFormat.Int
                 : o is long      ? StreamFormat.Long
                 : o is double    ? StreamFormat.Double
                 : o is RawInt128 ? StreamFormat.RawInt128
                 : o is decimal   ? StreamFormat.Decimal
                 : o is float     ? StreamFormat.Float
                 : o is short     ? StreamFormat.Short
                 : o is byte      ? StreamFormat.Byte
                 :                  StreamFormat.None;
        }


        #endregion

        #region Data members

        /// <summary>
        /// An underlying IComparer object (optional).
        /// </summary>
        protected IComparer m_comparer;

        #endregion

        #region Enum StreamFormat

        /// <summary>
        /// Serialization format.
        /// </summary>
        protected enum StreamFormat
        {
            /// <summary>
            /// Unknown value.
            /// </summary>
            None = -1,

            /// <summary>
            /// Null value.
            /// </summary>
            Null = 0,

            /// <summary>
            /// Integer value.
            /// </summary>
            Int = 1,

            /// <summary>
            /// Long value.
            /// </summary>
            Long = 2,

            /// <summary>
            /// Double value.
            /// </summary>
            Double = 3,

            /// <summary>
            /// RawInt128 value.
            /// </summary>
            RawInt128 = 4,

            /// <summary>
            /// Decimal value.
            /// </summary>
            Decimal = 5,

            /// <summary>
            /// Float value.
            /// </summary>
            Float = 14,

            /// <summary>
            /// Short value.
            /// </summary>
            Short = 15,

            /// <summary>
            /// Byte value.
            /// </summary>
            Byte = 16
        }

        #endregion
    }
}