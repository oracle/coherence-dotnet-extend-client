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
    /// A thread-safe <see cref="IDictionary"/>-based <see cref="ICollection"/>
    /// implementation that contains no duplicate elements.
    /// </summary>
    /// <author>Luk Ho  2012.08.27</author>
    public class SafeHashSet : DictionarySet
    {
        #region Constructors

        /// <summary>
        /// Create a new <c>SafeHashSet</c>.
        /// </summary>
        public SafeHashSet()
        {}

        /// <summary>
        /// Create a new <c>SafeHashSet</c> with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial capacity of the backing dictionary.
        /// </param>
        public SafeHashSet(int capacity)
            : this(new SynchronizedDictionary(capacity))
        { }

        /// <summary>
        /// Create and populate a new <c>SafeHashSet</c> with the given
        /// collection of elements.
        /// </summary>
        /// <param name="items">The collection of elements to populate the set
        /// with.</param>
        public SafeHashSet(ICollection items)
            : base(items)
        {}

        /// <summary>
        /// Create a new <c>SafeHashSet</c> that uses the specified IDictionary to
        /// store its elements.
        /// </summary>
        /// <param name="dict">The storage dictionary.</param>
        protected internal SafeHashSet(IDictionary dict)
            : base(dict)
        {}

        /// <summary>
        /// Initializes a new instance of the <c>SafeHashSet</c> class using the
        /// specified 
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// and <see cref="T:System.Runtime.Serialization.StreamingContext"/>.
        /// </summary>
        /// <param name="info">
        /// A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// object containing the information required to initialize this 
        /// <c>SafeHashSet</c> instance.
        /// </param>
        /// <param name="context">
        /// A <see cref="T:System.Runtime.Serialization.StreamingContext"/> 
        /// object containing the source and destination of the serialized 
        /// stream associated with this dictionary. 
        /// </param>
        protected SafeHashSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        #endregion

        #region Internal

        /// <summary>
        /// Factory pattern: Provide an underlying dictionary for this
        /// thread-safe Set implementation.
        /// </summary>
        /// <returns>
        /// A new thread-safe dictionary instance.
        /// </returns>
        protected override IDictionary InstantiateDictionary()
        {
            return new SynchronizedDictionary();
        }

        #endregion
    }
}
