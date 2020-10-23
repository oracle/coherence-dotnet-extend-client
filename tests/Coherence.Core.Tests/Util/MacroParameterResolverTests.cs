/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using NUnit.Framework;
using Tangosol.Util.Collections;

namespace Tangosol.Util
{
    public class MacroParameterResolverTests
    {
        #region Test Methods

        [Test]
        public void TestMacroReplacement()
        {
            IDictionary mapAttr = new HashDictionary();
            mapAttr.Add("str", "string");
            mapAttr.Add("int", "42");
            mapAttr.Add("flt", "42.0");
            mapAttr.Add("dcml", "42.0");
            mapAttr.Add("long", "42");
            mapAttr.Add("dbl", "42.0");
            mapAttr.Add("bl", "true");
            
            MacroParameterResolver resolver =
                new MacroParameterResolver(mapAttr);

            Assert.That(resolver.ResolveParameter("string", "{str}"), Is.EqualTo("string"));
            Assert.That(resolver.ResolveParameter("int", "{int}"), Is.EqualTo(42));
            Assert.That(resolver.ResolveParameter("float", "{flt}"), Is.EqualTo(42.0F));
            Assert.That(resolver.ResolveParameter("decimal", "{dcml}"), Is.EqualTo(42.0M));
            Assert.That(resolver.ResolveParameter("long", "{long}"), Is.EqualTo(42L));
            Assert.That(resolver.ResolveParameter("double", "{dbl}"), Is.EqualTo(42.0D));
            Assert.That(resolver.ResolveParameter("bool", "{bl}"), Is.True);
        }
        
        [Test]
        public void TestMacroReplacementWithDefaults()
        {
            MacroParameterResolver resolver =
                new MacroParameterResolver(null);

            Assert.That(resolver.ResolveParameter("string", "{str string}"), Is.EqualTo("string"));
            Assert.That(resolver.ResolveParameter("int", "{int 42}"), Is.EqualTo(42));
            Assert.That(resolver.ResolveParameter("float", "{flt 42.0}"), Is.EqualTo(42.0F));
            Assert.That(resolver.ResolveParameter("decimal", "{dcml 42.0}"), Is.EqualTo(42.0M));
            Assert.That(resolver.ResolveParameter("long", "{long 42}"), Is.EqualTo(42L));
            Assert.That(resolver.ResolveParameter("double", "{dbl 42.0}"), Is.EqualTo(42.0D));
            Assert.That(resolver.ResolveParameter("bool", "{bl false}"), Is.False);
        }
        
        [Test]
        public void TestMacroReplacementUnresolved()
        {
            MacroParameterResolver resolver =
                new MacroParameterResolver(null);

            Assert.That(() => resolver.ResolveParameter("string", "{str}"), Throws.ArgumentException);
        }
        
        [Test]
        public void TestMacroReplacementInvalidFormatNoOpeningCurlyBrace()
        {
            MacroParameterResolver resolver =
                new MacroParameterResolver(null);

            Assert.That(() => resolver.ResolveParameter("string", "str}"), Throws.ArgumentException);
        }
        
        [Test]
        public void TestMacroReplacementInvalidFormatNoClosingCurlyBrace()
        {
            MacroParameterResolver resolver =
                new MacroParameterResolver(null);

            Assert.That(() => resolver.ResolveParameter("string", "{str"), Throws.ArgumentException);
        }
        
        [Test]
        public void TestMacroReplacementNullType()
        {
            MacroParameterResolver resolver =
                new MacroParameterResolver(null);

            Assert.That(() => resolver.ResolveParameter(null, "{str"), 
                Throws.InstanceOf(typeof(ArgumentNullException)));
        }
        
        [Test]
        public void TestMacroReplacementNullValue()
        {
            MacroParameterResolver resolver =
                new MacroParameterResolver(null);

            Assert.That(() => resolver.ResolveParameter("string", null), 
                Throws.InstanceOf(typeof(ArgumentNullException)));
        }
        
        #endregion
    }
}