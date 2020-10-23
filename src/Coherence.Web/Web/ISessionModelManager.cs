/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Web.SessionState;

namespace Tangosol.Web
{
    /// <summary>
    /// Defines the methods that model managers have to implement.
    /// </summary>
    /// <remarks>
    /// Model manager is responsible for model serialization and
    /// deserialization. Session provider simply delegates all the 
    /// calls to a configured instance of model manager.
    /// </remarks>
    /// <author>Aleksandar Seovic  2008.10.07</author>
    public interface ISessionModelManager
    {
        /// <summary>
        /// The <b>SessionStateItemExpireCallback</b> delegate for the
        /// <b>Session_OnEnd</b> event as defined in the Global.asax file.
        /// </summary>
        SessionStateItemExpireCallback SessionEndCallback { get; set; }

        /// <summary>
        /// Create a new instance of an appropriate 
        /// <see cref="ISessionModel"/> for this model manager.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="ISessionModel"/>.
        /// </returns>
        ISessionModel CreateSessionModel();

        /// <summary>
        /// Load session from the cache.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        /// <param name="exclusive">
        /// Flag specifying whether to obtain exclusive access to the 
        /// specified session.
        /// </param>
        /// <returns>
        /// The session loaded from the cache.
        /// </returns>
        SessionHolder LoadSession(SessionKey sessionId, bool exclusive);

        /// <summary>
        /// Update session items in the cache and release the exclusive lock.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        /// <param name="holder">Session holder.</param>
        /// <param name="newSession">
        /// Flag specifying whether this is a new session.
        /// </param>
        void SaveSession(SessionKey sessionId, SessionHolder holder, bool newSession);

        /// <summary>
        /// Releases a lock on the session in a data store.
        /// </summary>
        /// <param name="sessionId">
        /// Session key for the current request.
        /// </param>
        /// <param name="lockId">
        /// The lock identifier for the current request.
        /// </param>
        void ReleaseSession(SessionKey sessionId, long lockId);

        /// <summary>
        /// Delete session from the cache.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        void RemoveSession(SessionKey sessionId);

        /// <summary>
        /// Reset session timeout.
        /// </summary>
        /// <param name="sessionId">Session key.</param>
        void ResetSessionTimeout(SessionKey sessionId);
    }
}