/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Net.Messaging
{
    /// <summary>
    /// Defines method Run() that executes action specific to the object in
    /// which it is implemented.
    /// </summary>
    /// <author>Ana Cikic  2006.08.22</author>
    public interface IRunnable
    {
        /// <summary>
        /// Execute the action specific to the object implementation.
        /// </summary>
        void Run();
    }
}