/*
 * Copyright (c) 2000, 2025, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * https://oss.oracle.com/licenses/upl.
 */
using System;

using NUnit.Framework;

namespace Tangosol.Util.Filter
{
    [TestFixture]
    public class FilterTriggerTests
    {
        [SetUp]
        public void SetUp()
        {
            TestContext.Error.WriteLine($"[START] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.Error.WriteLine($"[END]   {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {TestContext.CurrentContext.Test.FullName}");
        }

        [Test]
        public void TestFilterTrigger()
        {
            FilterTrigger ft1 = new FilterTrigger();
            Assert.IsNotNull(ft1);
            Assert.IsNull(ft1.Filter);
            Assert.AreEqual(FilterTrigger.ActionCode.Rollback, ft1.Action);

            FilterTrigger ft2 = new FilterTrigger(AlwaysFilter.Instance);
            Assert.IsNotNull(ft2);
            Assert.IsNotNull(ft2.Filter);
            Assert.IsInstanceOf(typeof(AlwaysFilter), ft2.Filter);
            Assert.AreEqual(FilterTrigger.ActionCode.Rollback, ft2.Action);

            FilterTrigger ft3 = new FilterTrigger(NeverFilter.Instance, FilterTrigger.ActionCode.Ignore);
            Assert.AreNotEqual(ft3, ft2);
            Assert.AreNotEqual(ft3.GetHashCode(), ft2.GetHashCode());
            Assert.IsNotNull(ft3);
            Assert.IsNotNull(ft3.Filter);
            Assert.IsInstanceOf(typeof(NeverFilter), ft3.Filter);
            Assert.AreEqual(FilterTrigger.ActionCode.Ignore, ft3.Action);

            TestCacheTriggerEntry entry = new TestCacheTriggerEntry("key", "value");
            Assert.IsNotNull(entry);
            Assert.AreEqual(entry.Value, entry.OriginalValue);
            entry.SetValue("newvalue", true);
            Assert.AreNotEqual(entry.Value, entry.OriginalValue);

            //action = ignore
            ft3.Process(entry);
            Assert.AreEqual(entry.Value, entry.OriginalValue);

            ft3 = new FilterTrigger(NeverFilter.Instance, FilterTrigger.ActionCode.Remove);
            Assert.IsFalse(entry.IsRemoved);
            ft3.Process(entry);
            Assert.IsTrue(entry.IsRemoved);

            ft3 = new FilterTrigger(NeverFilter.Instance, FilterTrigger.ActionCode.Rollback);
            Exception e = null;
            try
            {
                ft3.Process(entry);
            }
            catch (Exception ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.IsInstanceOf(typeof(ArgumentException), e);
        }
    }
}
