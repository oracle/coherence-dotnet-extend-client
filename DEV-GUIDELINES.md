<!--

  Copyright (c) 2000, 2020, Oracle and/or its affiliates.

  Licensed under the Universal Permissive License v 1.0 as shown at
  http://oss.oracle.com/licenses/upl.

-->
# Development Guidelines

This page provides information on how to successfully contribute to the Coherence .NET extend client. Coding conventions are stylistic in nature and the Coherence style is different to many open source projects therefore we understand the raising of eyebrows. However, consistency is significantly more important than the adopted subjective style, therefore please conform to the following rules as it has a direct impact on review times.


## Contents
1. [Coding Guidelines](#intro)
    1. [File Layout and Structure](#1)
    2. [Comment Rules](#2)
    3. [Interface conventions](#3)
    4. [Inheritance conventions](#4)
    5. [Indentation conventions](#5)
    6. [Variable prefix conventions](#6)
1. [Tools](#tools)


# <a name="intro"></a>Coding Guidelines

Except otherwise listed below, use the [Java Coding Guidelines](https://github.com/oracle/coherence/blob/master/DEV-GUIDELINES.md).

## <a name="1"></a>File Layout and Structure

* All source files end in the .cs suffix.

### Layout structure

* Use the .NET "`#region/#endregion`" instead of the "`// ----- xxx -----...`" style to separate logical sections

## <a name="2"></a>Comment rules

* Each class, method and data member must be preceded by a .NET style comment.

## <a name="3"></a>Interface conventions

* All interface names must start with "I" (uppercase i). For example: `IOperationalContext`

## <a name="4"></a>Inheritance conventions

The following rules apply to how objects should inherit from base classes:

* Always inherit Object virtually.
* Always inherit interfaces virtually (interfaces are pure virtual and must have an empty constructor).
* Avoid virtual inheritance except for the above two use cases.

## <a name="5"></a>Indentation conventions

* Each indent consists of 4 spaces (no literal tabs).
* Curly braces are on their own line, not indented from the parent (Visual Studio default setting). This is different than the Java coding guideline.
* Source lines within the curly braces must be indented 4 spaces (Visual Studio default setting). This is different than the Java coding guideline.


* Each indent consists of 4 spaces (no literal tabs).
* Curly braces are indented from the parent and on their own line.
*   Statements that may span multiple lines must be enclosed in braces (e.g. if/for/while/try). This implies that there must never be one or two line if blocks.

```C#
if (condition)
{
    code...
}
```

## <a name="6"></a>Variable prefix conventions

* Coherence .NET source DOES NOT use type prefixes for variable names
  * Good
```C#
    int count;
```
  * Bad
```C#
  int nCount;
```


# <a name="tools"></a>Tools

Visual Studio 2008 or later
