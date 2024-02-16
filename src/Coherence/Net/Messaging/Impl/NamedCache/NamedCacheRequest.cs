/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using Tangosol.Net.Messaging.Impl;

namespace Tangosol.Net.Messaging.Impl.NamedCache
{
    /// <summary>
    /// Base class for all NamedCache Protocol <see cref="Request"/>
    /// messages.
    /// </summary>
    /// <seealso cref="Request"/>
    /// <seealso cref="NamedCacheProtocol"/>
    /// <author>Ivan Cikic  2006.08.25</author>
    public abstract class NamedCacheRequest : Request
    {}
}