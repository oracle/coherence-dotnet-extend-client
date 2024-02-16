/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System.Collections.Generic;



namespace Tangosol.IO.Pof

{
    /// <summary>
    /// Storage for evolvable classes.
    /// </summary>
    /// <author>Aleksandar Seovic  2013.11.04</author>
    /// <since>Coherence 12.2.1</since>
    public class EvolvableHolder
    {
        /// <summary>
        /// Return <see cref="IEvolvable"/> for the specified type id.
        /// </summary>
        /// <param name="typeId">Type identifier</param>
        /// <returns>IEvolvable instance</returns>
        public IEvolvable GetEvolvable(int typeId)
        {
            if (m_evolvableMap.ContainsKey(typeId))
            {
                return m_evolvableMap[typeId];
            }
            IEvolvable e = new SimpleEvolvable(0);
            m_evolvableMap[typeId] = e;
            return e;
        }

        /// <summary>
        /// Return type identifiers for all the IEvolvables within this holder.
        /// </summary>
        /// <returns>
        /// Type identifiers for all the Evolvables within this holder.
        /// </returns>
        public ICollection<int> TypeIds
        {
            get { return m_evolvableMap.Keys; }
        }

        /// <summary>
        /// Return <code>True</code> if this holder is empty.
        /// </summary>
        /// <returns>
        /// <code>True</code> if this holder is empty, <code>False</code> otherwise.
        /// </returns>
        public bool IsEmpty
        {
            get { return m_evolvableMap.Count == 0; }
        }

        /// <summary>
        /// Map of evolvables.
        /// </summary>
        private readonly IDictionary<int, IEvolvable> m_evolvableMap = new Dictionary<int, IEvolvable>();
    }
}
