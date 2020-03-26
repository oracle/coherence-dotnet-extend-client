/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

namespace Tangosol.IO.Pof.Reflection.Internal
{
    /// <summary>
    /// Visitor pattern description. This pattern implementation is targeted 
    /// at builders that require <see cref="Type"/> information.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <typeparam name="B">The type to pass to the visitor.</typeparam>
    /// <since>Coherence 3.7.1</since>
    public interface IVisitor<B>
    {
        /// <summary>
        /// Visit the given builder <c>B</c> and optionally mutate it using
        /// information contained within the given Type.
        /// </summary>
        /// <param name="builder">
        /// The builder being visited.
        /// </param>
        /// <param name="type">
        /// The Type used to enrich the builder.
        /// </param>
        void Visit(B builder, Type type);
    }

    #region Inner interface: IRecipient

    /// <summary>
    /// A recipient informs a visitor of it's willingness to be visited.
    /// </summary>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <typeparam name="B">The type to pass to the visitor.</typeparam>
    /// <since>Coherence 3.7.1</since>
    public interface IRecipient<B>
    {
        /// <summary>
        /// Accept the given visitor.
        /// </summary>
        /// <param name="visitor">
        /// IVisitor that is requesting to visit this recipient.
        /// </param>
        /// <param name="type">
        /// The Type that can be used by the visitor.
        /// </param>
        void Accept(IVisitor<B> visitor, Type type);
    }

    #endregion
}
