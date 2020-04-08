/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.IO;

namespace Tangosol.IO.Pof.Reflection
{
    /// <summary>
    /// Abstract base class for static, path-based implementations of
    /// <see cref="IPofNavigator"/> interface.
    /// </summary>
    /// <author>Aleksandar Seovic  2009.03.30</author>
    /// <since>Coherence 3.5</since>
    public abstract class AbstractPofPath : IPofNavigator, IPortableObject
    {
        #region Abstract methods

        /// <summary>
        /// Return a collection of path elements.
        /// </summary>
        /// <returns>
        /// A collection of path elements.
        /// </returns>
        protected abstract int[] GetPathElements();

        #endregion

        #region Implementation of IPofNavigator interface

        /// <summary>
        /// Locate the <see cref="IPofValue"/> designated by this IPofNavigator 
        /// within the passed IPofValue.
        /// </summary>
        /// <param name="valueOrigin">
        /// The origin from which navigation starts.
        /// </param>
        /// <returns>
        /// The resulting IPofValue.
        /// </returns>
        /// <exception cref="PofNavigationException">
        /// If the navigation fails; for example one of the intermediate nodes 
        /// in this path is a "terminal" IPofValue.
        /// </exception>
        public IPofValue Navigate(IPofValue valueOrigin)
        {
            int[] aiPathElements = GetPathElements();
            IPofValue valueCurrent = valueOrigin;

            for (int i = 0, c = aiPathElements.Length; i < c; i++)
            {
                valueCurrent = valueCurrent.GetChild(aiPathElements[i]);
            }
            return valueCurrent;
        }

        #endregion

        #region Abstract implementation of IPortableObject interface

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
        public abstract void ReadExternal(IPofReader reader);

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
        public abstract void WriteExternal(IPofWriter writer);

        #endregion
    }
}