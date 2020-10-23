/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using Tangosol.Net.Cache;
using Tangosol.Util;
using Tangosol.Util.Filter;

namespace Tangosol.Net
{
    /// <summary>
    /// The ViewBuilder provides a means to #build() a view (ContinuousQueryCache)
    /// using a fluent pattern / style.
    /// </summary>
    /// <see cref="ContinuousQueryCache"/>
    /// <author>rl 6.3.19</author>
    /// <since>12.2.1.4</since>
    public class ViewBuilder
    {
        #region Constructors

        /// <summary>
        /// Construct a new ViewBuilder for the provided INamedCache.
        /// </summary>
        /// <param name="cache">
        /// The INamedCache from which the view will be created.
        /// </param>
        public ViewBuilder(INamedCache cache)
            : this(() => cache)
        {
        }

        /// <summary>
        /// Construct a new ViewBuilder for the provided INamedCache.
        /// The function should return a new NamedCache instance upon
        /// each invocation.
        /// </summary>
        /// <param name="supplierCache">
        /// The function returning an INamedCache from which the view will be created.
        /// </param>
        public ViewBuilder(Func<INamedCache> supplierCache)
        {
            m_supplierCache = supplierCache;
        }

        #endregion

        #region Builder methods

        /// <summary>
        /// The IFilter that will be used to define the entries maintained in this view.
        /// If no IFilter is specified, AlwaysFilter will be used.
        /// </summary>
        /// <param name="filter">
        /// The IFilter that will be used to query the underlying INamedCache.
        /// </param>
        /// <returns>This ViewBuilder.</returns>
        public ViewBuilder Filter(IFilter filter)
        {
            m_filter = filter;
            return this;
        }

        /// <summary>
        /// The ICacheListener that will receive all events, including those that
        /// result from the initial population of the view.
        /// </summary>
        /// <param name="listener">
        /// the ICacheListener that will receive all the events from
        /// the view, including those corresponding to its initial
        /// population.
        /// </param>
        /// <returns>This ViewBuilder.</returns>
        public ViewBuilder Listener(ICacheListener listener)
        {
            m_listener = listener;
            return this;
        }

        /// <summary>
        /// The IValueExtractor that this view will use to transform the results from
        /// the underlying cache prior to storing them locally.
        /// </summary>
        /// <param name="mapper">
        /// The IValueExtractor that will be used to
        /// transform values retrieved from the underlying cache
        /// before storing them locally; if specified, this
        /// view will become read-only.
        /// </param>
        /// <returns>This ViewBuilder.</returns>
        public ViewBuilder Map(IValueExtractor mapper)
        {
            m_mapper = mapper;
            return this;
        }

        /// <summary>
        /// The resulting view will only cache keys.
        /// </summary>
        /// <remarks>
        /// NOTE: this is mutually exclusive with #values().
        /// </remarks>
        /// <returns>This ViewBuilder.</returns>
        public ViewBuilder Keys()
        {
            m_fCacheValues = false;
            return this;
        }

        /// <summary>
        /// The resulting view will cache both keys and values.
        /// </summary>
        /// <remarks>
        /// NOTE: this is mutually exclusive with #keys().
        /// </remarks>
        /// <returns>This ViewBuilder.</returns>
        public ViewBuilder Values()
        {
            m_fCacheValues = true;
            return this;
        }

        /// <summary>
        /// Construct a view of the INamedCache provided to this builder.
        /// </summary>
        /// <returns>The view of the NamedCache provided to this builder.</returns>
        public INamedCache Build()
        {
            IFilter filter = m_filter;
            return new ContinuousQueryCache(m_supplierCache,
                                            filter == null ? AlwaysFilter.Instance : filter,
                                            m_fCacheValues,
                                            m_listener,
                                            m_mapper);
        }

        #endregion

        #region Data members

        /// <summary>
        /// The Supplier returning a NamedCache from which the
        /// view will be created.
        /// </summary>
        private Func<INamedCache> m_supplierCache;

        /// <summary>
        /// The Filter that will be used to define the entries maintained
        /// in this view.
        /// </summary>
        private IFilter m_filter;

        /// <summary>
        /// The MapListener that will receive all the events from
        /// the view, including those corresponding to its initial
        /// population.
        /// </summary>
        private ICacheListener m_listener;

        /// <summary>
        /// The ValueExtractor that will be used to transform values
        /// retrieved from the underlying cache before storing them locally; if
        /// specified, this view will become read-only.
        /// </summary>
        private IValueExtractor m_mapper;

        /// <summary>
        /// Flag controlling if the {@code view} will cache both keys and values
        /// or only keys.
        /// </summary>
        private bool m_fCacheValues;

        #endregion
    }
}