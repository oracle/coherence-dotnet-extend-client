/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */

namespace Tangosol.Web;

/// <summary>
/// Represents the Coherence session configuration options.
/// </summary>
/// <author>Vaso Putica  2024.06.06</author>
public class CoherenceSessionOptions
{
    /// <summary>
    /// Name of the session cache.
    /// </summary>
    public string CacheName => "aspnet-session-storage";

    /// <summary>
    /// Name of the overflow session cache.
    /// </summary>
    public string OverflowCacheName => "aspnet-session-overflow";

    /// <summary>
    /// Type of the session.
    /// </summary>
    public HttpSessionType SessionType { get; set; } = HttpSessionType.Monolithic;

    /// <summary>
    /// Attribute size limit after which attribute will be stored in overflow cache.
    /// </summary>
    public int MinOverflowAttributeSize { get; set; } = 1024;

    /// <summary>
    /// Path to the Coherence configuration file.
    /// </summary>
    public string CoherenceConfig { get; set; } = "assembly://Coherence/Tangosol.Config/coherence.xml";
    /// <summary>
    /// Path to the Coherence cache configuration file.
    /// </summary>
    public string CacheConfig { get; set; } = "assembly://Coherence/Tangosol.Config/coherence-aspnet-cache-config.xml";

    /// <summary>
    /// Overrides default application ID that is used as a part of session key.
    /// </summary>
    public string ApplicationId { get; set; }

    /// <summary>
    /// Key to access Coherence configuration subsection.
    /// </summary>
    public static readonly string CONFIG = "CoherenceSession";

    /// <summary>
    /// Coherence session type constants.
    /// </summary>
    public enum HttpSessionType
    {
        Monolithic,
        Split
    }
}