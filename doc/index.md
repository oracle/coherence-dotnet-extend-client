# .NET Client API Reference for Oracle Coherence

14c (14.1.2.0.0)

F79657-01

November 2024

This is documentation for the .NET 6.0 or later implementation of Oracle Coherence Client for .NET.

### [API Documentation](api/)

### About Oracle Coherence Client for .NET

**Oracle Coherence Client for .NET** allows .NET applications to access Coherence clustered services, including data, data events, and data processing from outside the Coherence cluster. Typical uses of Coherence for .NET include desktop and web applications that require access to Coherence-managed data.

Coherence Client for .NET is a lightweight .NET library that connects to a Coherence\*Extend clustered service instance running within the Coherence cluster using a high performance TCP/IP-based communication layer. This library sends all client requests to the Coherence\*Extend clustered service which, in turn, responds to client requests by delegating to an actual Coherence clustered service (for example, a *Partitioned* cache service).

An `INamedCache` instance is retrieved via the `CacheFactory.GetCache()` API call. Once it is obtained, a client accesses the `INamedCache` in the same way as it would if it were part of the Coherence cluster. The fact that cache operations are being sent and executed on a remote cluster node (over TCP/IP) is completely transparent to the client application.

### Additional Information

* [Oracle Coherence CE Web Site](https://coherence.community/)
* [Oracle Coherence CE Blog](https://medium.com/oracle-coherence)
* [Oracle Coherence YouTube Channel](https://www.youtube.com/user/OracleCoherence)
* [Oracle Coherence CE Source Code](https://github.com/oracle/coherence)
* [Oracle Coherence for .NET Source Code](https://github.com/oracle/coherence-dotnet-extend-client)
