/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

namespace Tangosol.Util
{
    internal static class GateFactory
    {
        /// <summary>
        /// Construct a new <see cref="Gate"/> instance.
        /// </summary>
        public static Gate NewGate
        {
            get 
            {
                return new ThreadGateSlim();
            }
        }
    }
}
