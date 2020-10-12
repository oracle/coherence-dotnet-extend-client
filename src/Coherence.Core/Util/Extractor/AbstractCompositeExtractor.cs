/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Diagnostics;
using System.IO;
using System.Text;

using Tangosol.IO.Pof;
using Tangosol.Util;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// Abstract base class for <see cref="IValueExtractor"/> implementations
    /// that are based on an underlying array of <b>IValueExtractor</b>
    /// objects.
    /// </summary>
    /// <author>Gene Gleyzer  2006.02.08</author>
    /// <author>Ivan Cikic  2006.10.20</author>
    /// <since>Coherence 3.2</since>
    public abstract class AbstractCompositeExtractor : AbstractExtractor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Obtain the <see cref="IValueExtractor"/> array.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b> array.
        /// </value>
        public virtual IValueExtractor[] Extractors
        {
            get { return m_extractors; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AbstractCompositeExtractor()
        {}

        /// <summary>
        /// Construct a <b>AbstractCompositeExtractor</b> based on the
        /// specified <see cref="IValueExtractor"/> array.
        /// </summary>
        /// <param name="extractors">
        /// The <see cref="IValueExtractor"/> array.
        /// </param>
        public AbstractCompositeExtractor(IValueExtractor[] extractors)
        {
            Debug.Assert(extractors != null);
            m_extractors = extractors;
        }

        #endregion

        #region IPorabaleObject implemetation

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
            m_extractors = (IValueExtractor[]) reader.ReadArray(0, EMPTY_EXTRACTOR_ARRAY);
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
            writer.WriteArray(0, m_extractors);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Compare the <b>AbstractCompositeExtractor</b> with another object
        /// to determine equality.
        /// </summary>
        /// <remarks>
        /// Two <b>AbstractCompositeExtractor</b> objects are considered
        /// equal iff they belong to the same class and their underlying
        /// <see cref="IValueExtractor"/> arrays are deep-equal.
        /// </remarks>
        /// <param name="o">
        /// The object to compare with.
        /// </param>
        /// <returns>
        /// <b>true</b> iff this <b>AbstractCompositeExtractor</b> and the
        /// passed object are equivalent.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is AbstractCompositeExtractor)
            {
                AbstractCompositeExtractor that = (AbstractCompositeExtractor) o;
                return GetType() == that.GetType() && CollectionUtils.EqualsDeep(m_extractors, that.m_extractors);
            }
            return false;
        }

        /// <summary>
        /// Determine a hash value for the AbstractCompositeExtractor object
        /// according to the general <b>object.GetHashCode</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <see cref="IValueExtractor"/>
        /// object.
        /// </returns>
        public override int GetHashCode()
        {
            int               hash       = 0;
            IValueExtractor[] extractors = m_extractors;
            foreach (IValueExtractor ve in extractors)
            {
                hash += ve.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Return a human-readable description for this
        /// <see cref="IValueExtractor"/>.
        /// </summary>
        /// <returns>
        /// A <b>String</b> description of the <see cref="IValueExtractor"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetType().Name).Append('(');

            IValueExtractor[] extractors = m_extractors;
            for (int i = 0, c = extractors.Length; i < c; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(extractors[i]);
            }
            sb.Append(')');
            return sb.ToString();
        }

        #endregion

        #region Constants

        /// <summary>
        /// Empty array of <see cref="IValueExtractor"/> objects.
        /// </summary>
        private static readonly IValueExtractor[] EMPTY_EXTRACTOR_ARRAY = new IValueExtractor[0];

        #endregion

        #region Data members

        /// <summary>
        /// The <see cref="IValueExtractor"/> array.
        /// </summary>
        protected IValueExtractor[] m_extractors;

        #endregion
    }
}