/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.IO.Pof
{
    /// <summary>
    /// Extension of the <see cref="IPortableObject"/> interface that
    /// supports forwards- and backwards-compatibility of POF data streams.
    /// </summary>
    /// <author>Cameron Purdy, Jason Howes  2006.07.14</author>
    /// <author>Aleksandar Seovic  2006.08.12</author>
    /// <since>Coherence 3.2</since>
    public interface IEvolvablePortableObject : IPortableObject, IEvolvable
    {}
}