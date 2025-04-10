<!--

  Copyright (c) 2000, 2025, Oracle and/or its affiliates.

  Licensed under the Universal Permissive License v 1.0 as shown at
  https://oss.oracle.com/licenses/upl.

-->

-----
<img src=https://oracle.github.io/coherence/assets/images/logo-red.png><img>

![CI Build](https://github.com/oracle/coherence-dotnet-extend-client/workflows/CI%20Build/badge.svg)
[![License](http://img.shields.io/badge/license-UPL%201.0-blue.svg)](https://oss.oracle.com/licenses/upl/)

# Oracle Coherence for .NET - Community Edition

## Contents
1. [Introduction to Coherence](#intro)
1. [How to Get Coherence Community Edition](#acquire)
1. [Getting Started](#getting-started)
1. [Building](#build)
1. [CLI Hello Coherence Example](#started)
1. [Testing](#testing)
1. [Documentation](#documentation)
1. [Contributing](#contributing)

## <a name="intro"></a>Introduction to Coherence

[Coherence](http://coherence.community/) is a scalable, fault-tolerant, cloud-ready,
distributed platform for building grid-based applications and reliably storing data.
The product is used at scale, for both compute and raw storage, in a vast array of 
industries such as critical financial trading systems, high performance telecommunication
products and eCommerce applications. 

Typically these deployments do not tolerate any downtime and Coherence is chosen due to its 
novel features in death detection, application data evolvability, and the robust,
battle-hardened core of the product that enables it to be seamlessly deployed and 
adapted within any ecosystem.

At a high level, Coherence provides an implementation of the familiar `IDictionary`
interface but rather than storing the associated data in the local process it is partitioned
(or sharded) across a number of designated remote nodes. This partitioning enables
applications to not only distribute (and therefore scale) their storage across multiple
processes, machines, racks, and data centers but also to perform grid-based processing
to truly harness the CPU resources of the machines. 

The Coherence interface `INamedCache` (an extension of `IDictionary`) provides methods
to query, aggregate (map/reduce style) and compute (send functions to storage nodes
for locally executed mutations) the data set. These capabilities, in addition to 
numerous other features, enable Coherence to be used as a framework for writing robust,
distributed applications.

## <a name="acquire"></a>How to Get Coherence Community Edition

For more details on how to obtain and use Coherence, please see the Coherence CE [README](https://github.com/oracle/coherence/tree/main/README.md).

## Getting Started

Coherence for .NET allows .NET applications to access Coherence clustered services, including data, data events, and data processing from outside the Coherence cluster. Typical uses of Coherence for .NET include desktop and web applications that require access to Coherence caches.

Coherence for .NET consists of a lightweight .NET library that connects to a Coherence clustered service instance running within the Coherence cluster using a high performance TCP/IP-based communication layer. This library sends all client requests to the Coherence clustered proxy service which, in turn, responds to client requests by delegating to an actual Coherence clustered service (for example, a Partitioned or Replicated cache service).

See the [documentation](#documentation) for details on building Coherence applications using .NET.

## <a name="build"></a>Building

### Prerequisites and Dependencies

1. Microsoft .NET 6.0 or higher runtime and SDK
2. Microsoft Visual Studio 2022+, or Visual Studio Code with the NET plugin installed is required to build

The Coherence for .NET also depends on [docfx](https://dotnet.github.io/docfx/) to build documentation.

The following additional dependencies are required for testing:
1. Java 17 or later

To build Coherence for .NET, you must run the dotnet build utility, passing in the desired target that you would like to execute.
Using .NET 6, the output from the build is located in the `src/<project>/bin/<Debug|Release>/net6.0` subdirectory.
Using .NET 8, the output from the build is located in the `src/<project>/bin/<Debug|Release>/net8.0` subdirectory.

To build Coherence clone this repository and run the following commands:

For debug build run:
```
dotnet build
```
The resulting files:

`src/Coherence/bin/Debug/net6.0/Coherence.dll`
`src/Coherence.SessionStore/bin/Debug/net6.0/Coherence.SessionStore.dll`

For release build run:
```
dotnet build --configuration Release
```
The resulting files:

`src/Coherence/bin/Release/net6.0/Coherence.dll`
`src/Coherence.SessionStore/bin/Release/net6.0/Coherence.SessionStore.dll`

`src/Coherence/bin/Release/Coherence.14.1.2.nupkg` - nuget package

To clean all build artifacts from your build system, run the following
command:

```
dotnet clean
```

## <a name="started"></a>CLI Hello Coherence Example
The following example illustrates starting a storage enabled Coherence server, followed by running the HelloCoherence console application. The HelloCoherence application inserts and retrieves data from the Coherence server.

### Build HelloCoherence
1. Using dotnet-cli to create a HelloCoherence console application:
```
dotnet new console --name "HelloCoherence"
```
1. Add the following references to the HelloCoherence.csproj (provide the Coherence.dll location in the `<HintPath>`):
```
  <ItemGroup>
    <Reference Include="Coherence, Version=14.1.2.3, Culture=neutral, PublicKeyToken=0ada89708fdf1f9a, processorArchitecture=MSIL">
      <HintPath>Coherence.dll</HintPath>
    </Reference>
    <PackageReference Include="Common.Logging" Version="3.4.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="6.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="6.0.0" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="6.0.1" />
  </ItemGroup>
```
Also include any Coherence configuration files you may have.

1. Replace Program.cs code with the following source:
```
/*
 * Copyright (c) 2000, 2024, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Run.Xml;
namespace Hello
{
    class Program
    {
        static void Main(string[] args)
        {
            // Display title as the C# console Coherence app and
            // show user the valid commands:
            Console.WriteLine("Coherence for .NET Extend Client");
            Console.WriteLine("The following are the available cache operations:");
            Console.WriteLine("\tcache <cacheName> - specify a cache name to use");
            Console.WriteLine("\tput <key> <value> - put a <key, value> pair into the cache");
            Console.WriteLine("\tget <key> - get the value of a given key from the cache");
            Console.WriteLine("\tremove <key> - remove an entry of the given key from the cache");
            Console.WriteLine("\tlist - list all the entries in the cache");
            Console.WriteLine("\tsize - get the size of the cache");
            Console.WriteLine("\tbye - exit the console");
            Console.WriteLine();
            Console.Write("Map (?): ");

            // Declare variabs.
            String      cacheName  = null;
            INamedCache namedCache = null;
            String      op         = Console.ReadLine().ToLower();
            String[]    opList     = op.Split();

            // Processing cache operations.
            while (opList[0].CompareTo("bye") != 0)
            {
                String key;
                String value;

                if (!opList[0].Equals("cache") && namedCache == null)
                {
                    Console.WriteLine("No named cache.  Please specify a named cache to use.");
                }
                else
                {
                    switch (opList[0])
                    {
                        case "cache":
						    if (opList.Length < 2)
							{
								Console.WriteLine("No cache name.  Please specify a cache name to use.");
							}
							else
							{
								cacheName = opList[1];
								namedCache = CacheFactory.GetCache(cacheName);
							}
                            break;

                        case "put":
						    if (opList.Length < 3)
							{
								Console.WriteLine("No key/value pair.  Please specify the key and value to be put into the cache.");
							}
							else
							{
								key = opList[1];
								value = opList[2];
								namedCache[key] = value;
							}
                            break;

                        case "get":
						    if (opList.Length < 2)
							{
								Console.WriteLine("No key.  Please specify the key to get.");
							}
							else
							{
								key = opList[1];
								var result = namedCache[key];
								Console.WriteLine(result == null ? "NULL" : namedCache[key]);
							}
                            break;

                        case "remove":
						    if (opList.Length < 2)
							{
								Console.WriteLine("No key.  Please specify the key to remove.");
							}
							else
							{
								key = opList[1];
								namedCache.Remove(key);
							}
                            break;

                        case "list":
                            foreach (ICacheEntry entry in namedCache.Entries)
                            {
                                Console.WriteLine(entry.Key + " = " + entry.Value);
                            }
                            break;

                        case "size":
                            Console.WriteLine(namedCache.Count);
                            break;

                        default:
                            Console.WriteLine("Valid operations are: cache, put, get, remove, list, size, and bye.");
                            break;
                    }
                }

                Console.WriteLine("");
                if (namedCache == null)
                {
                    Console.Write("Map (?): ");
                }
                else
                {
                    Console.Write("Map (" + cacheName + "): ");
                }

                // Read cache operation
                op = Console.ReadLine().ToLower();
                opList = op.Split();
            }
        }
    }
}
```

By default, you need to provide a POF configure file, pof-config.xml, in the TargetFramework directory. Below are a sample pof-config.xml file:

```
<?xml version="1.0"?>
<!--
  Copyright (c) 2000, 2020, Oracle and/or its affiliates.

  Licensed under the Universal Permissive License v 1.0 as shown at
  https://oss.oracle.com/licenses/upl.
-->
<pof-config xmlns="http://schemas.tangosol.com/pof">
  <user-type-list>
    <!-- include all "standard" Coherence POF user types -->
    <include>assembly://Coherence/Tangosol.Config/coherence-pof-config.xml</include>

    <!-- include all application POF user types -->
  </user-type-list>
</pof-config>
```

4. Build the HelloCoherence project
```
dotnet build
```

### Start a Coherence server

```
"%JAVA_HOME%\bin\java" -Dcoherence.pof.enabled=true -Dcoherence.log.level=9 -jar coherence.jar
```

### Run the Hello Coherence example

```shell script
dotnet run
```

```
Coherence for .NET Extend Client
The following are the available cache operations:
        cache <cacheName> - specify a cache name to use
        put <key> <value> - put a <key, value> pair into the cache
        get <key> - get the value of a given key from the cache
        remove <key> - remove an entry of the given key from the cache
        list - list all the entries in the cache
        size - get the size of the cache
        bye - exit the console
		
Map (?): cache welcomes

Map (welcomes): get english
NULL

Map (welcomes): put english hello

Map (welcomes): put spanish hola

Map (welcomes): put french bonjour

Map (welcomes): get english
Hello

Map (welcomes): list
french = bonjour
english = hello
spanish = hola

Map (welcomes): bye
```

```
dotnet run
```

```
Coherence for .NET Extend Client
The following are the available cache operations:
        cache <cacheName> - specify a cache name to use
        put <key> <value> - put a <key, value> pair into the cache
        get <key> - get the value of a given key from the cache
        remove <key> - remove an entry of the given key from the cache
        list - list all the entries in the cache
        size - get the size of the cache
        bye - exit the console
		
Map (?): cache welcomes

Map (welcomes): list
french = bonjour
english = hello
spanish = hola

Map (welcomes): bye
```

### <a name="testing"></a>Testing

To run Coherence for .NET test suite, first you must run a Coherence server. Go to tests/test-server folder and start server:

```
cd tests/test-server
mvn clean package -Dcoherence.groupid=com.oracle.coherence.ce -Drevision=24.09 && mvn exec:exec -Dcoherence.groupid=com.oracle.coherence.ce -Drevision=24.09 -Dmain=com.tangosol.net.Coherence
```

To run the test suite (excluding ASP.NET session tests that require the commercial edition of Coherence), use the following command:

```
dotnet test --filter FullyQualifiedName\!~Tangosol.Web
```

## Documentation

To build Coherence for .NET API documentation, run the following command.  The API documentation can be viewed using Microsoft help viewer.
```
cd doc
docfx docfx.json
```

For further details on developing Coherence for .NET applications, see the documentation [here](https://docs.oracle.com/en/middleware/fusion-middleware/coherence/14.1.2/develop-remote-clients/creating-net-extend-clients.html).

## Contributing

This project welcomes contributions from the community. Before submitting a pull request, please [review our contribution guide](./CONTRIBUTING.md)

## Security

Please consult the [security guide](./SECURITY.md) for our responsible security vulnerability disclosure process.

## License

Copyright (c) 2020, 2024 Oracle and/or its affiliates.

Released under the Universal Permissive License v1.0 as shown at
<https://oss.oracle.com/licenses/upl/>.
