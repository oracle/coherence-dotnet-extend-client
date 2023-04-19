<!--

  Copyright (c) 2000, 2023, Oracle and/or its affiliates.

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
1. [Introduction to Coherence for .NET](#intro-extend)
1. [Building](#build)
1. [CLI Hello Coherence Example](#started)
1. [Testing](#testing)
1. [Documentation](#docs)
1. [Contributing](#contrib)

# <a name="intro"></a>Introduction to Coherence

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

# <a name="acquire"></a>How to Get Coherence Community Edition

For more details on how to obtain and use Coherence, please see the Coherence CE [README](https://github.com/oracle/coherence/tree/master/README.md).

# <a name="intro_extend"></a>Introduction to Coherence for .NET

Coherence for .NET allows .NET applications to access Coherence clustered services, including data, data events, and data processing from outside the Coherence cluster. Typical uses of Coherence for .NET include desktop and web applications that require access to Coherence caches.

Coherence for .NET consists of a lightweight .NET library that connects to a Coherence clustered service instance running within the Coherence cluster using a high performance TCP/IP-based communication layer. This library sends all client requests to the Coherence clustered proxy service which, in turn, responds to client requests by delegating to an actual Coherence clustered service (for example, a Partitioned or Replicated cache service).

See the [documentation](#docs) for details on building Coherence applications using .NET.

# <a name="build"></a>Building

### Prerequisites and Dependencies

1. Microsoft .NET 4.0 or higher runtime and SDK
2. Supported Microsoft Windows operating system (see the systemrequirements for the appropriate .NET runtime above)
3. Microsoft Visual Studio 2010+, or Visual Studio Code with the NET plugin installed is required to build

The Coherence for .NET also depends on the following libraries and software:
1. [Common.Logging, 2.0.0.0](#commonlogging)
1. [MSBuild.Extension.Pack, 1.9.1](#msbuildex)
1. [Sandcastle Help File Builder and Tools, 2019.11.17](#shfb)
1. [Microsoft Build Tools 2015](#msbuildtools)

#### <a name="commonlogging"></a>Common.Logging 2.0.0.0
<a name="intro"></a>
Download and install Common.Logging 2.0.0.0 (`https://www.nuget.org/packages/Common.Logging/2.0.0`) or later. Copy Common.Logging.2.0.0\lib\2.0 to lib\net\2.0.

#### <a name="msbuildex"></a>MSBuild.Extension.Pack.1.9.1
Download and install MSBuild.Extension.Pack, 1.9.1 (`https://www.nuget.org/packages/MSbuild.Extension.Pack/1.9.0`). Copy MSBuild.Extension.Pack.1.9.1 to tools\internal\msbuild.

#### <a name="shfb"></a>Sandcastle Help File Builder and Tools, 2019.11.17
Coherence uses Sandcastle Help File Builder and Tools to build the Coherence .NET documentation. Down load and install Sandcastle Help File Builder and Tools, 2019.11.17 (`https://github.com/EWSoftware/SHFB/releases`). Then copy the "Sandcastle Help File Builder" directory 
to tools\internal\shfb.

#### <a name="msbuildtools"></a>Microsoft Build Tools 2015
Sandcastle Help File Builder and Tools requires Microsoft Build Tools 2015.  You can down load Microsoft Build Tools 2015（`https://www.microsoft.com/en-us/download/details.aspx?id=48159`）or later and install it if you don't have it already.

The following additional dependencies are required for testing:
1. [NUnit 2 releases, 2.6.2](#nunit)
1. [NUnit.Runners, 2.6.2](#nunitrunners)
1. [Ant, 1.7.0](#ant)
1. Java 1.8 or later
1. [WinHttpCertCfg.exe](#httpcerts)

#### <a name="nunit"></a>NUnit 2.6.2
Download and install NUnit, 2.6.2 (`https://nunit.org/download/#olderReleases`) or later. Copy NUnit.2.6.2 to tools\internal\nunit

#### <a name="nunitrunners"></a>NUnit.Runners, 2.6.2
Download and install NUnit.Runners, 2.6.2 (`https://www.nuget.org/packages/NUnit.Runners/2.6.2`) or later. Copy NUnit.Runners.2.6.2 to tools\internal\NUnit.Runners

#### <a name="ant"></a>Ant
Download and install Ant, 1.7.0 or later. Then copy it to under tools\internal\ant.

#### <a name="httpcerts"></a>WinHttpCertCfg.exe
Download and install WinHttpCertCfg.exe ('https://www.microsoft.com/en-us/download/details.aspx?id=19801`).  Then copy it to tools\internal\resourcekit.

You can use NuGet Package Manager through Visual Studio or Develooper Command Prompt to download most of the dependency libraries and software.

If C:\coherence-net is your project root directory, it should contain the following directories

- C:\coherence-net\lib\net\2.0
- C:\coherence-net\tools\cluster-control
- C:\coherence-net\tools\internal\ant
- C:\coherence-net\tools\internal\cluster-control
- C:\coherence-net\tools\internal\msbuild
- C:\coherence-net\tools\internal\msbuild-custom
- C:\coherence-net\tools\internal\nunit
- C:\coherence-net\tools\internal\nunit.runners
- C:\coherence-net\tools\internal\resourcekit
- C:\coherence-net\tools\internal\shfb
- C:\coherence-net\tools\msbuild-custom

The Coherence for .NET build system is based upon msbuild. To build Coherence for .NET, you must run the msbuild build utility, passing in the desired target that you would like to execute.
The output from the build are in the build subdirectory.

To build Coherence for .NET, start a "Developer Command Prompt for VS" 2017 or 2019.
Clone this repository and run the following command:
```
set JAVA_HOME=<Java Home Path>
```
```
bin\cfgbuild.cmd
```
```
msbuild /t:build Coherence.msbuild
```
The resulting files:

`build\Coherence.2010\Debug` - debug build

`build\Coherence.2010\Release` - release build

To clean all build artifacts from your build system, run the following
command:

```
msbuild /t:clean Coherence.msbuild
```

# <a name="started"></a>CLI Hello Coherence Example
The following example illustrates starting a storage enabled Coherence server, followed by running the HelloCoherence console application. The HelloCoherence application inserts and retrieves data from the Coherence server.

## Build HelloCoherence
1. Using dotnet-cli to create a HelloCoherence console application:
```
dotnet new console -name "HelloCoherence"
```
1. Add the following references to the HelloCoherence.csproj (provide the Coherence.Core.dll location in the `<HintPath>`):
```
  <ItemGroup>
    <Reference Include="Coherence.Core, Version=14.1.1.13, Culture=neutral, PublicKeyToken=0ada89708fdf1f9a, processorArchitecture=MSIL">
      <HintPath>Coherence.Core.dll</HintPath>
    </Reference>
    <PackageReference Include="Common.Logging" Version="3.4.1" />
    <PackageReference Include="System.Configuration.ConfigurationManager" Version="4.7.0" />
  </ItemGroup>
```
Also include any Coherence configuration files you may have.

1. Replace Program.cs code with the following source:
```
/*
 * Copyright (c) 2000, 2022, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
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
  Copyright (c) 2000, 2022, Oracle and/or its affiliates.

  Licensed under the Universal Permissive License v 1.0 as shown at
  http://oss.oracle.com/licenses/upl.
-->
<pof-config xmlns="http://schemas.tangosol.com/pof">
  <user-type-list>
    <!-- include all "standard" Coherence POF user types -->
    <include>assembly://Coherence.Core/Tangosol.Config/coherence-pof-config.xml</include>

    <!-- include all application POF user types -->
  </user-type-list>
</pof-config>
```

4. Build the HelloCoherence project
```
dotnet build
```

## Start a Coherence server

```
"%JAVA_HOME%\bin\java" -Dcoherence.pof.enabled=true -Dcoherence.log.level=9 -jar coherence.jar
```

## Run the Hello Coherence example

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

Map (welcomes): put english Hello

Map (welcomes): put spanish Hola

Map (welcomes): put french Bonjour

Map (welcomes): get english
Hello

Map (welcomes): list
french = Bonjour
english = Hello
spanish = Hola

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
french = Bonjour
english = Hello
spanish = Hola

Map (welcomes): bye
```

# <a name="testing"></a>Testing

To run Coherence for .NET test suite, you must have a coherence.jar.  Using ant, you can provide a build.properties file in the tools\ant directory to specify a maven repository from which coherence.jar can be downloaded.
The test suite starts a Coherence server for the .NET clients to connect to run the tests.

```
msbuild /t:test Coherence.msbuild
```

To run Coherence for .NET test suite starting a Coherence server in docker container, use the following commaond:

```
msbuild /t:test Coherence.docker
```

# <a name="docs"></a>Documentation

To build Coherence for .NET API documentation, run the following command.  The API documentation can be viewed using Microsoft help viewer.
```
msbuild /t:doc Coherence.msbuild
```
To build Coherence for .NET installable package, use the following command.  The command produces a Coherence.msi in the build directory that can be used to install Coherence for .NET.
```
msbuild /t:dist Coherence.msbuild
```
The resulting files:

`dist\14.1.2.0b0` - Coherence MSI installer

For further details on developing Coherence for .NET applications, see the documentation [here](https://docs.oracle.com/en/middleware/standalone/coherence/14.1.1.0/develop-remote-clients/creating-net-extend-clients.html).

# <a name="contrib"></a>Contribute

Interested in contributing?  Please see our contribution [guidelines](CONTRIBUTING.md) for details.

# Security

Please consult the [security guide](./SECURITY.md) for our responsible security vulnerability disclosure process
