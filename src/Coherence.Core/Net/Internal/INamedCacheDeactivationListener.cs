/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;

namespace Tangosol.Net.Internal
{
    /// <summary>
    /// Pseudo <b>MapListener</b> that can be used to listen for a deactivation event
    /// from a <b>NamedCache</b>.
    /// </summary>
    /// <remarks>
    /// Instances of this interface can be added to a NamedCache with the single
    /// parameter <see cref="IObservableCache.AddCacheListener(ICacheListener)"/> method.
    /// When the NamedCache is deactivated, it will call the
    /// <see cref="ICacheListener.EntryDeleted"/> method.
    /// When the NamedCache is truncated, it will call the
    /// <see cref="ICacheListener.EntryUpdated"/> method.
    /// </remarks>
    public interface INamedCacheDeactivationListener : CacheListenerSupport.ISynchronousListener
    {
    }
}
