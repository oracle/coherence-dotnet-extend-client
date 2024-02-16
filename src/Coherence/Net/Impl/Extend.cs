/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Text;

namespace Tangosol.Net.Impl
{
    /// <summary>
    /// Base class for all Coherence*Extend implementation classes.
    /// </summary>
    /// <since>12.2.1.3</since>
    public abstract class Extend
    {
        #region Extend implementation

        /// <summary>
        /// Return a human-readable description of this class.
        /// </summary>
        /// <returns>
        /// A string representation of this class.
        /// </returns>
        protected virtual string GetDescription()
        {
            return "";
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// Returns a string representation of this class.
        /// </summary>
        /// <returns>
        /// A string representation of this Extend class.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(GetType().Name);

            string s;
            try
            {
                s = GetDescription();
            }
            catch (Exception /* e */)
            {
                s = null;
            }

            if (String.IsNullOrEmpty(s))
            {
                sb.Append('@').Append(GetHashCode());
            }
            else
            {
                sb.Append('(').Append(s).Append(')');
            }

            return sb.ToString();
        }

        #endregion
    }
}
