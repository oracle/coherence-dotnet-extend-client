/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Coherence.Tests")]
[assembly: AssemblyDescription(".NET Client API Tests for Oracle Coherence")]
[assembly: AssemblyCompany("Oracle")]
[assembly: AssemblyProduct("Oracle Coherence.NET Tests")]
[assembly: AssemblyCopyright("Copyright (c) 2000, 2020, Oracle and/or its affiliates. All rights reserved.")]

// sign assembly when RELEASE configuration is used, in order to get access to Coherence internals
#if RELEASE
[assembly: AssemblyKeyFile("../../keys/Coherence-AssemblyKey.snk")]
#endif

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers
// by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
