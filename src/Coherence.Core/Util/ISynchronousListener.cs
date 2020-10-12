/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
namespace Tangosol.Util
{
    /// <summary>
    /// A tag interface indicating that a listener implementation has to receive
    /// the event notifications synchronously on the corresponding service's thread.
    /// </summary>
    /// <remarks>
    /// This interface should be considered as a very advanced feature, as
    /// a listener implementation that is marked as a SynchronousListener
    /// must exercise extreme caution during event processing since any delay
    /// with return or unhandled exception will cause a delay or complete
    /// shutdown of the corresponding service.
    /// </remarks>    
    public interface ISynchronousListener 
    {
    }
}