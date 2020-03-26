/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Collections;

using Tangosol.IO.Pof;
using Tangosol.Net.Cache;

namespace Tangosol.Util.Extractor
{
    /// <summary>
    /// An IndexAwareExtractor implementation that is only used to create a
    /// <see cref="ConditionalIndex"/>.
    /// <para>
    /// The underlying IValueExtractor is used for value extraction during 
    /// index creation and is the extractor that is associated with the created
    /// ConditionalIndex in the given index map. Using the ConditionalExtractor 
    /// to extract values in not supported.
    /// </para>
    /// </summary>
    /// <author>Tom Beerbower  2010.02.08</author>
    /// <author>Jason Howes  2010.10.04</author>
    public class ConditionalExtractor : AbstractExtractor, IIndexAwareExtractor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// The filter used by this extractor.
        /// </summary>
        protected IFilter Filter { get; private set; }

        /// <summary>
        /// The underlying extractor.
        /// </summary>
        protected IValueExtractor Extractor { get; private set; }

        /// <summary> 
        /// Specifies whether or not this extractor will create a
        /// <see cref="ConditionalIndex"/> that supports a forward index.
        /// </summary>
        protected bool IsForwardIndexSupported { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct the ConditionalExtractor.
        /// </summary>
        public ConditionalExtractor()
        {    
        }

        /// <summary>
        /// Construct the ConditionalExtractor.
        /// </summary>
        /// <param name="filter">
        /// The filter used by this extractor to create a 
        /// <see cref="ConditionalIndex"/>; must not be null.
        /// </param>
        /// <param name="extractor">
        /// The extractor used by this extractor to create a 
        /// <see cref="ConditionalIndex"/>; the created index will be 
        /// associated with this extractor in the given index map; must
        /// not be null.
        /// </param>
        /// <param name="forwardIndex">
        /// Specifies whether or not this extractor will create a 
        /// <see cref="ConditionalIndex"/> that supports a forward index.
        /// </param>
        public ConditionalExtractor(IFilter filter, IValueExtractor extractor,
                bool forwardIndex)
        {
            Debug.Assert(filter != null && extractor != null,
                         "Filter and extractor must not be null");

            Filter                  = filter;
            Extractor               = extractor;
            IsForwardIndexSupported = forwardIndex;
        }

        #endregion

        #region IndexAwareExtractor interface

        /// <summary>
        /// Create an index and associate it with the corresponding extractor.
        /// <para>
        /// Important: it is a responsibility of this method's implementations
        /// to place the necessary &lt;IValueExtractor, ICacheEntry&gt; entry 
        /// into the given map of indexes.
        /// </para>
        /// </summary>
        /// <param name="ordered">
        /// <c>true</c> iff the contents of the indexed information should be 
        /// ordered; <c>false</c> otherwise.
        /// </param>
        /// <param name="comparer">
        /// The IComparator object which imposes an ordering of entries in the 
        /// index contents; or <c>null</c> if the entries' values natural 
        /// ordering should be used.
        /// </param>
        /// <param name="dict">
        /// IDictionary to be updated with the created index.
        /// </param>
        /// <returns>
        /// The created index; <c>null</c> if the index has not been created.
        /// </returns>
        public ICacheIndex CreateIndex(bool ordered, IComparer comparer,
                IDictionary dict)
        {
            var extractor = Extractor;
            var index     = (ICacheIndex) dict[extractor];

            if (index != null)
            {
                if (index is ConditionalIndex
                  && Equals((index as ConditionalIndex).Filter, Filter))
                {
                    return null;
                }
                throw new ArgumentException(
                        "Repetitive AddIndex call for " + this);
            }

            index = new ConditionalIndex(Filter, extractor, ordered,
                    comparer, IsForwardIndexSupported);
            dict[extractor] = index;
            return index;
        }

        /// <summary>
        /// Destroy an existing index and remove it from the given dictionary
        /// of indexes. 
        /// </summary>
        /// <param name="dict">
        /// IDictionary to be updated by removing the index being destroyed.
        /// </param>
        /// <returns>
        /// The destroyed index; <c>null</c> if the index does not exist.
        /// </returns>
        public ICacheIndex DestroyIndex(IDictionary dict)
        {
            var index = (ICacheIndex) dict[Extractor];
            dict.Remove(Extractor);
            return index;
        }

        #endregion

        #region IValueExtractor interface

        /// <summary>
        /// Using a ConditionalExtractor to extract values is not supported.
        /// </summary>
        /// <param name="oTarget">
        /// An object to retrieve the value from.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// ConditionalExtractor may not be used as an extractor.
        /// </exception>
        /// <returns>
        /// The extracted value as an object; <c>null</c> is an acceptable
        /// value.
        /// </returns>
        public override object Extract(object oTarget)
        {
            throw new InvalidOperationException(
                "ConditionalExtractor may not be used as an extractor.");
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Deserialize model using specified reader.
        /// </summary>
        /// <param name="reader">Reader to use.</param>
        public void ReadExternal(IPofReader reader)
        {
            Filter    = (IFilter) reader.ReadObject(0);
            Extractor = (IValueExtractor) reader.ReadObject(1);
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
        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteObject(0, Filter);
            writer.WriteObject(1, Extractor);
        }
        #endregion

        #region Object methods

        /// <summary>
        /// Compare the ConditionalExtractor with another object to determine
        /// equality.
        /// </summary>
        /// <remarks>
        /// Two ConditionalExtractor objects are considered equal iff their 
        /// underlying <b>IFilters</b>, <b>IValueExtractors</b> and support
        /// for forward indices are equal.
        /// </remarks>
        /// <param name="o">
        /// The reference object with which to compare.
        /// </param>
        /// <returns>
        /// <b>true</b> if this ConditionalExtractor and the passed object are
        /// equivalent.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is ConditionalExtractor)
            {
                ConditionalExtractor that = (ConditionalExtractor) o;
                return Equals(Filter, that.Filter) &&
                    Equals(Extractor, that.Extractor) &&
                    IsForwardIndexSupported == that.IsForwardIndexSupported;
            }

            return false;
        }

        /// <summary>
        /// Determine a hash value for the ConditionalExtractor object
        /// according to the general <b>Object.GetHashCode</b> contract.
        /// </summary>
        /// <returns>
        /// An integer hash value for this ConditionalExtractor object.
        /// </returns>
        public override int GetHashCode()
        {
            return Filter.GetHashCode() ^ Extractor.GetHashCode();
        }

       /// <summary>
        /// Return a human-readable description for this ConditionalExtractor.
        /// </summary>
        /// <returns>a String description of the ConditionalExtractor</returns>
        public override string ToString()
        {
            return "ConditionalExtractor" +
                "(extractor=" + Extractor + ", filter=" + Filter + ")";
        }

        #endregion
    }
}