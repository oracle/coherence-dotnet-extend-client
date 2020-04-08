/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Util;

namespace Tangosol.Util
{
    /// <summary>
    /// IValueManipulator represents a composition of
    /// <see cref="IValueExtractor"/> and <see cref="IValueUpdater"/>
    /// implementations.
    /// </summary>
    /// <author>Gene Gleyzer  2005.10.31</author>
    /// <author>Goran Milosavljevic  2006.10.20</author>
    /// <since>Coherence 3.1</since>
    public interface IValueManipulator
    {
        /// <summary>
        /// Retreive the underlying <see cref="IValueExtractor"/> reference.
        /// </summary>
        /// <value>
        /// The <b>IValueExtractor</b>.
        /// </value>
        IValueExtractor Extractor { get; }

        /// <summary>
        /// Retreive the underlying <see cref="IValueUpdater"/> reference.
        /// </summary>
        /// <value>
        /// The <b>IValueUpdater</b>.
        /// </value>
        IValueUpdater Updater { get; }
    }
}