/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Collections;
using System.Runtime.Serialization;

namespace Tangosol.Util.Collections
{
    /// <summary>
    /// <see cref="IDictionary"/>-based <see cref="ICollection"/> implementation 
    /// that contains no duplicate elements.
    /// </summary>
    /// <author>Jason Howes  2010.09.30</author>
    /// <author>Luk Ho  2012.08.27</author>
    public class HashSet : DictionarySet
    {
        #region Constructors

        /// <summary>
        /// Create a new <c>HashSet</c>.
        /// </summary>
        public HashSet()
        {}

        /// <summary>
        /// Create a new <c>HashSet</c> with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial capacity of the backing dictionary.
        /// </param>
        public HashSet(int capacity)
            : this(new HashDictionary(capacity))
        {}

        /// <summary>
        /// Create and populate a new <c>HashSet</c> with the given collection
        /// of elements.
        /// </summary>
        /// <param name="items">The collection of elements to populate the set
        /// with.</param>
        public HashSet(ICollection items)
            : base(items)
        {}

        /// <summary>
        /// Create a new <c>HashSet</c> that uses the specified IDictionary to
        /// store its elements.
        /// </summary>
        /// <param name="dict">The storage dictionary.</param>
        protected internal HashSet(IDictionary dict)
            : base(dict)
        {}

        /// <summary>
        /// Initializes a new instance of the <c>HashSet</c> class using the
        /// specified 
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// and <see cref="T:System.Runtime.Serialization.StreamingContext"/>.
        /// </summary>
        /// <param name="info">
        /// A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// object containing the information required to initialize this 
        /// <c>HashSet</c> instance.
        /// </param>
        /// <param name="context">
        /// A <see cref="T:System.Runtime.Serialization.StreamingContext"/> 
        /// object containing the source and destination of the serialized 
        /// stream associated with this dictionary. 
        /// </param>
        protected HashSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {}

        #endregion
    }
}
