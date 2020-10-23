/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Net;

namespace Tangosol.Net
{
    /// <summary>
    /// SingleAddressProvider is is an implementation of the
    /// <see cref="IAddressProvider"/> interface that wraps a single address 
    /// dynamically added to this provider.
    /// </summary>
    /// <author>Wei Lin  2012.05.31</author>
    /// <since>Coherence 12.1.2</since>
    public class SingleAddressProvider : IAddressProvider
    {
        #region Constructors

        /// <summary>
        /// Set the SocketAddress for this <see cref="IAddressProvider"/>.
        /// </summary>
        /// <param name="address">
        /// the address which will be returned by this <see cref="IAddressProvider"/>
        /// </param>
        public SingleAddressProvider(IPEndPoint address)
        {
            m_address        = address;
            m_isAddressGiven = false;
        }

        #endregion

        #region IAddressProvider implementation

        /// <summary>
        /// Next available address to use.
        /// </summary>
        /// <remarks>
        /// If the caller can successfully use the returned address (e.g. a
        /// connection was established), it should call the
        /// IAddressProvider's <see cref="IAddressProvider.Accept"/> method.
        /// </remarks>
        /// <value>
        /// The next available address or <c>null</c> if the list of
        /// available addresses was exhausted.
        /// </value>
        public virtual IPEndPoint NextAddress
        {
            get
            {
                // toggle m_fAddressGiven in case of reuse
                return (m_isAddressGiven = !m_isAddressGiven) ? m_address : null;
            }
        }

        /// <summary>
        /// This method should be called by the client immediately after it
        /// determines that it can successfully use an address returned by
        /// the <see cref="IAddressProvider.NextAddress"/>.
        /// </summary>
        public virtual void Accept()
        {
            // no-op
        }

        /// <summary>
        /// This method should be called by the client immediately after it
        /// determines that an attempt to use an address returned by the
        /// <see cref="IAddressProvider.NextAddress"/> has failed.
        /// </summary>
        /// <param name="eCause">
        /// (Optional) an exception that carries the reason why the caller
        /// rejected the previously returned address.
        /// </param>
        public virtual void Reject(Exception eCause)
        {
            // no-op
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Return a string representation of this
        /// ConfigurableAddressProvider.
        /// </summary>
        /// <returns>
        /// A string representation of the list of configured addresses.
        /// </returns>
        public override string ToString()
        {
            return "Address=" + m_address;
        }


        #endregion

        #region Data members

        /// <summary>
        /// The address returned by this <see cref="IAddressProvider"/>.
        /// </summary>
        private IPEndPoint m_address;

        /// <summary>
        /// Whether the address has already been returned.
        /// </summary>
        private bool m_isAddressGiven;

        #endregion
    }
}
