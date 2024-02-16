/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Tangosol.Util.Collections
{
    /// <summary>
    /// <see cref="IDictionary"/>-based <see cref="ICollection"/> implementation 
    /// that contains no duplicate elements and maintains sorted values.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    [Serializable]
    public class SortedHashSet : HashSet
    {
        /// <summary>
        /// Create a new <c>SortedHashSet</c>.
        /// </summary>
        public SortedHashSet()
            : base(new SortedDictionary())
        {
        }

        /// <summary>
        /// Create a new <c>SortedHashSet</c> with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        /// The initial capacity of the backing dictionary.
        /// </param>
        public SortedHashSet(int capacity)
            : base(new SortedDictionary(capacity))
        {
        }

        /// <summary>
        /// Create and populate a new <c>SortedHashSet</c> with the given collection
        /// of elements.
        /// </summary>
        /// <param name="items">The collection of elements to populate the set
        /// with.</param>
        public SortedHashSet(ICollection items) 
            : this(items.Count)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Create a new <c>SortedHashSet</c> with the specified <see cref="IComparer"/>
        /// </summary>
        /// <param name="comparer">
        /// Specify an <see cref="IComparer"/> to determine element order 
        /// opposed to using the natural ordering of the objects.
        /// </param>
        public SortedHashSet(IComparer comparer)
            : base(new SortedDictionary(comparer))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>SortedHashSet</c> class 
        /// using the specified 
        /// <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// and <see cref="T:System.Runtime.Serialization.StreamingContext"/>.
        /// </summary>
        /// <param name="info">
        /// A <see cref="T:System.Runtime.Serialization.SerializationInfo"/> 
        /// object containing the information required to initialize this 
        /// <c>SortedHashSet</c> instance.
        /// </param>
        /// <param name="context">
        /// A <see cref="T:System.Runtime.Serialization.StreamingContext"/> 
        /// object containing the source and destination of the serialized 
        /// stream associated with this dictionary. 
        /// </param>
        protected SortedHashSet(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
