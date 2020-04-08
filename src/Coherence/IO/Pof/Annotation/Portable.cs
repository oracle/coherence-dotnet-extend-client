/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.IO.Pof.Annotation
{
    /// <summary>
    /// Portable marks a class as being eligible for use by a
    /// <see cref="PofAnnotationSerializer"/>. This annotation is only permitted at the
    /// class level and is a marker annotation with no members. The following class
    /// illustrates how to use <see cref="Portable"/> and <see cref="PortableProperty"/>
    /// annotations.
    /// </summary>
    /// <remarks>
    /// <code>
    /// [Portable]
    /// public class Person
    /// {
    ///     [PortableProperty(0)]
    ///     public string GetFirstName()
    ///     {
    ///         return m_firstName;
    ///     }
    /// 
    ///     [PortableProperty(1)]
    ///     public string LastName
    ///     {
    ///         get; set;
    ///     }
    ///
    ///     private String m_firstName;
    ///     [PortableProperty(2)]
    ///     private int m_age;
    /// }
    /// </code>
    /// </remarks>
    /// <author>Harvey Raja  2011.07.25</author>
    /// <since>Coherence 3.7.1</since>
    /// <see>PortableProperty</see>
    [AttributeUsage(AttributeTargets.Class)]
    public class Portable : Attribute
    {
    }
}