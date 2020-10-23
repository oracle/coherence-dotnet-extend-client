/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿namespace Tangosol.IO.Pof

{
    /// <summary>
    /// Defines an interface that should be implemented by the classes that want to
    /// support evolution.
    /// </summary>
    /// <author>Aleksandar Seovic  2013.11.04</author>
    /// <since>Coherence 12.2.1</since>
    public interface IEvolvableObject
    {
        /// <summary>
        /// Return <see cref="IEvolvable"/> holder object for the specified type id.
        /// </summary>
        /// <remarks>
        /// This method should only return Evolvable instance if the specified type
        /// id matches its own type id. Otherwise, it should delegate to the parent:
        /// 
        /// <example>
        ///     // assuming type ID of this class is 1234
        ///     private IEvolvable evolvable = new SimpleEvolvable(1234);
        ///     ...
        ///     public IEvolvable GetEvolvable(int nTypeId)
        ///     {
        ///         if (1234 == nTypeId)
        ///         {
        ///             return this.evolvable;
        ///         }
        ///
        ///         return base.GetEvolvable(nTypeId);
        ///     }
        /// </example>
        /// </remarks>
        /// <param name="nTypeId">
        /// Type id to get <see cref="IEvolvable"/> instance for.
        /// </param>
        /// <returns>
        /// IEvolvable instance for the specified type id.
        /// </returns>
        IEvolvable GetEvolvable(int nTypeId);

        /// <summary>
        /// Return <see cref="EvolvableHolder"/> that should be used to store information
        /// about evolvable objects that are not known during deserialization.
        /// </summary>
        /// <remarks>
        /// For example, it is possible to evolve the class hierarchy by adding new
        /// classes at any level in the hierarchy. Normally this would cause a problem
        /// during deserialization on older clients that don't have new classes at all,
        /// but EvolvableHolder allows us to work around that issue and simply store
        /// type id to opaque binary value mapping within it.
        /// </remarks>
        /// <returns>EvolvableHolder instance.</returns>
        EvolvableHolder GetEvolvableHolder();
    }
}
