/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using System.Collections;

using Tangosol.Util;
using Tangosol.Util.Extractor;
using Tangosol.Util.Filter;
using Tangosol.Util.Collections;

using NUnit.Framework;

namespace Tangosol.Net.Cache
{
    /// <summary>
    /// ConditionalIndex unit tests
    /// </summary>
    /// <author> tb 2010.2.08 </author>
    /// <author> lh 2010.6.09 </author>
    [TestFixture]
    public class ConditionalIndexTest
    {
        /// <summary>
        /// Test getValueExtractor
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testGetValueExtractor()
        {
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new GreaterFilter(extractor, 5);
            var index = new ConditionalIndex(filter, extractor, false, null, true);

            Assert.AreEqual(extractor, index.ValueExtractor);
        }

        /// <summary>
        /// Test getFilter
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testGetFilter()
        {
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new GreaterFilter(extractor, 5);
            var index = new ConditionalIndex(filter, extractor, false, null, true);

            Assert.AreEqual(filter, index.Filter);
        }

        /// <summary>
        /// Test isOrdered
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testIsOrdered()
        {
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new GreaterFilter(extractor, 5);
            var index = new ConditionalIndex(filter, extractor, false, null, true);

            Assert.IsFalse(index.IsOrdered);

            index = new ConditionalIndex(filter, extractor, true, null, true);

            Assert.IsTrue(index.IsOrdered);
        }

        /// <summary>
        /// Test isPartial
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testIsPartial()
        {
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new GreaterFilter(extractor, 5);
            var index = new ConditionalIndex(filter, extractor, false, null, true);

            Assert.IsFalse(index.IsPartial);

            index.Insert(new CacheEntry("key1", 6));
            Assert.IsFalse(index.IsPartial);

            index.Insert(new CacheEntry("key2", 10));
            Assert.IsFalse(index.IsPartial);

            index.Insert(new CacheEntry("key3", 4));
            Assert.IsTrue(index.IsPartial);

            index = new ConditionalIndex(filter, extractor, false, null, true);
            Assert.IsFalse(index.IsPartial);

            index.Insert(new CacheEntry("key1", 2));
            Assert.IsTrue(index.IsPartial);

            index.Insert(new CacheEntry("key2", 6));
            Assert.IsTrue(index.IsPartial);

            index = new ConditionalIndex(filter, extractor, false, null, true);

            index.Insert(new CacheEntry("key1", 8));
            Assert.IsFalse(index.IsPartial);

            index.Update(new CacheEntry("key1", 2));
            Assert.IsTrue(index.IsPartial);
        }

        /// <summary>
        /// Test getIndexContents
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testGetIndexContents()
        {
            var map = new HashDictionary
                          {
                              {"one", 1},
                              {"another_one", 1},
                              {"one_more", 1},
                              {"two", 2},
                              {"three", 3},
                              {"four", 4},
                              {"four_again", 4},
                              {"five", 5},
                              {"five_a", 5},
                              {"five_b", 5},
                              {"five_c", 5},
                              {"five_d", 5}
                          };

            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new LessFilter(extractor, 5);
            ConditionalIndex index = createIndex(map, filter, extractor, true);
            IDictionary indexContents = index.IndexContents;

            var setOne = indexContents[1] as HashSet;
            Assert.IsNotNull(setOne);
            Assert.AreEqual(3, setOne.Count);
            Assert.IsTrue(setOne.Contains("one"));
            Assert.IsTrue(setOne.Contains("another_one"));
            Assert.IsTrue(setOne.Contains("one_more"));

            var setTwo = indexContents[2] as HashSet;
            Assert.IsNotNull(setTwo);
            Assert.AreEqual(1, setTwo.Count);
            Assert.IsTrue(setTwo.Contains("two"));

            var setThree = indexContents[3] as HashSet;
            Assert.IsNotNull(setThree);
            Assert.AreEqual(1, setThree.Count);
            Assert.IsTrue(setThree.Contains("three"));

            var setFour = indexContents[4] as HashSet;
            Assert.IsNotNull(setFour);
            Assert.AreEqual(2, setFour.Count);
            Assert.IsTrue(setFour.Contains("four"));
            Assert.IsTrue(setFour.Contains("four_again"));

            var setFive = indexContents[5] as ICollection;
            Assert.IsNull(setFive);
        }

        /// <summary>
        /// Test get
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testGet()
        {
            IDictionary map = new HashDictionary {{"one", 1}, {"two", 2}, 
                {"three", 3}, {"four", 4}, {"five", 5}};

            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new LessFilter(extractor, 5);
            ConditionalIndex index = createIndex(map, filter, extractor, true);

            Assert.AreEqual(1, index.Get("one"));
            Assert.AreEqual(2, index.Get("two"));
            Assert.AreEqual(3, index.Get("three"));
            Assert.AreEqual(4, index.Get("four"));
            Assert.AreEqual(ObjectUtils.NO_VALUE, index.Get("five"));

            // forward map support == false
            index = createIndex(map, filter, extractor, false);

            Assert.AreEqual(ObjectUtils.NO_VALUE, index.Get("one"));
            Assert.AreEqual(ObjectUtils.NO_VALUE, index.Get("two"));
            Assert.AreEqual(ObjectUtils.NO_VALUE, index.Get("three"));
            Assert.AreEqual(ObjectUtils.NO_VALUE, index.Get("four"));
            Assert.AreEqual(ObjectUtils.NO_VALUE, index.Get("five"));
        }

        /// <summary>
        /// Test insert into a ConditionalIndex.  Verify the following :
        /// 1) the index contains a value for a key after an insert
        /// 2) if multiple equivalent values are inserted into the index
        ///    for different keys, verify that only one copy of the value exists in
        ///    the index
        /// 3) extracted values from entries that do not pass the filter test are
        ///    not added to the index
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testInsert()
        {
            // create the ConditionalIndex to be tested
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new LessFilter(extractor, 15);
            var mapIndex = new ConditionalIndex(filter, extractor, true, null, true);

            // define the keys and values for the mock entries
            const string oKey = "key";
            Object oValue = 11;
            const string oKey2 = "key2";
            Object oValue2 = oValue;
            const string oKey3 = "key3";
            Object oValue3 = 25;
            ICacheEntry entry = new CacheEntry(oKey, oValue);
            ICacheEntry entry2 = new CacheEntry(oKey2, oValue2);
            ICacheEntry entry3 = new CacheEntry(oKey3, oValue3);

            // begin test

            // verify that the index does not contain a value for the tested keys
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey2));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey3));

            // assert the the inverse map does not contain an entry for the extracted values
            Assert.IsFalse(mapIndex.IndexContents.Contains(oValue));
            Assert.IsFalse(mapIndex.IndexContents.Contains(oValue2));

            // insert into the index
            mapIndex.Insert(entry);
            mapIndex.Insert(entry2);
            mapIndex.Insert(entry3);

            // verify the value from the index for key
            object oIndexValue = mapIndex.Get(oKey);
            Assert.AreEqual(oValue, oIndexValue);

            // verify the value from the index for key2
            object oIndexValue2 = mapIndex.Get(oKey2);
            Assert.AreEqual(oValue2, oIndexValue2);

            // verify no value in the index for key3
            object oIndexValue3 = mapIndex.Get(oKey3);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue3);

            // verify that the value for key and key2 is the same instance
            Assert.AreSame(oIndexValue, oIndexValue2);

            // get the inverse map
            var mapInverse = (SynchronizedDictionary)mapIndex.IndexContents;

            // get the entry from the inverse map keyed by the extracted value
            var inverseEntry = mapInverse[oValue] as HashSet;
            Assert.IsNotNull(inverseEntry);
            Assert.IsNotEmpty(inverseEntry);
            Assert.IsTrue(inverseEntry.Contains(oKey));

            // get the entry from the inverse map keyed by the extracted value
            var inverseEntry2 = mapInverse[oIndexValue2] as HashSet;
            
            Assert.IsNotNull(inverseEntry2);
            Assert.IsNotEmpty(inverseEntry2);

            // verify that the set of keys contains key
            Assert.IsTrue(inverseEntry2.Contains(oKey2));

            // get the set of keys from the inverse map keyed by the extracted
            // value for key
            var set = mapInverse[oIndexValue] as HashSet;
            Assert.IsNotNull(set);

            // verify that the set of keys contains key
            Assert.IsTrue(set.Contains(oKey));

            // get the set of keys from the inverse map keyed by the extracted
            // value for key2
            set = mapInverse[oIndexValue2] as HashSet;
            Assert.IsNotNull(set);

            // verify that the set of keys contains key2
            Assert.IsTrue(set.Contains(oKey2));
        }

        /// <summary>
        /// Test insert into a ConditionalIndex.  Verify the following :
        /// 1) the index contains a value for a key after an insert
        /// 2) if multiple equivalent values are inserted into the index
        ///    for different keys, verify that only one copy of the value exists in
        ///    the index
        /// 3) extracted values from entries that do not pass the filter test are
        ///    not added to the index
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testInsert_forwardIndexFalse()
        {
            // create the ConditionalIndex to be tested
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new LessFilter(extractor, 15);
            var mapIndex = new ConditionalIndex(filter, extractor, true, null, false);

            // define the keys and values for the mock entries
            const string oKey = "key";
            Object oValue = 1;
            const string oKey2 = "key2";
            Object oValue2 = 2;
            const string oKey3 = "key3";
            Object oValue3 = 25;
            ICacheEntry entry = new CacheEntry(oKey, oValue);
            ICacheEntry entry2 = new CacheEntry(oKey2, oValue2);
            ICacheEntry entry3 = new CacheEntry(oKey3, oValue3);

            // verify that the index does not contain a value for the tested keys
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey2));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey3));

            // insert into the index
            mapIndex.Insert(entry);
            mapIndex.Insert(entry2);
            mapIndex.Insert(entry3);

            // all gets should return NO_VALUE since forward index is not supported
            // verify no value from the index for key
            object oIndexValue = mapIndex.Get(oKey);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue);

            // verify no value from the index for key2
            object oIndexValue2 = mapIndex.Get(oKey2);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue2);

            // verify no value in the index for key3
            object oIndexValue3 = mapIndex.Get(oKey3);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue3);

            // verify that the value for key and key2 is the same instance
            Assert.AreSame(oIndexValue, oIndexValue2,
                   "The value for key and key2 should be the same instance.");

            // get the inverse map
            IDictionary mapInverse = mapIndex.IndexContents;

            // get the entry from the inverse map keyed by the extracted value
            var inverseEntry = mapInverse[oValue] as HashSet;
            Assert.IsNotNull(inverseEntry);

            // verify that the set of keys contains key
            Assert.IsTrue(inverseEntry.Contains(oKey),
                          "The index's inverse map should contain the key.");
        }

        /// <summary>
        /// Test update on a ConditionalIndex.  Verify the following :
        /// 1) the index contains the new value for a key after an update
        /// 2) if multiple equivalent values are inserted into the index
        ///    for different keys, verify that only one copy of the value exists in
        ///    the index
        /// 3) extracted values from entries that do not pass the filter test are
        ///    not added to the index
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testUpdate()
        {
            // create the ConditionalIndex to be tested
            IValueExtractor extractor = new IdentityExtractor();
            IFilter         filter    = new LessFilter(extractor, 15);
            var             mapIndex  = new ConditionalIndex(filter, extractor, true, null, true);

            // define the keys and values
            const string oKey           = "key";
            Object       oValue         = 0;
            Object       oExtracted     = 0;
            const string oKey2          = "key2";
            Object       oValue2        = 11;
            Object       oExtracted2    = 11;
            const string oKey3          = "key3";
            Object       oValue3        = 21;
            Object       oExtracted3    = 21;
            Object       oNewValue      = oValue2;
            Object       oExtractedNew  = 11;
            Object       oNewValue2     = 30;
            Object       oExtractedNew2 = 30;
            ICacheEntry  entry          = new CacheEntry(oKey, oValue);
            ICacheEntry  entry2         = new CacheEntry(oKey2, oValue2);
            ICacheEntry  entry3         = new CacheEntry(oKey3, oValue3);
            ICacheEntry  entryNew       = new CacheEntry(oKey, oNewValue, oValue);
            ICacheEntry  entryNew2      = new CacheEntry(oKey2, oNewValue2, oValue2);

            // begin test

            // verify that the index does not contain a value for the tested keys
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey2));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey3));

            // insert into the index
            mapIndex.Insert(entry);   //key  (extracted value : 0)
            mapIndex.Insert(entry2);  //key2 (extracted value : 2)
            mapIndex.Insert(entry3);  //key3 (extracted value : 21)

            // verify the value from the index for key
            object oIndexValue = mapIndex.Get(oKey);
            Assert.AreEqual(oExtracted, oIndexValue,
                "The index should contain the extracted value for key.");

            // verify the value from the index for key2
            object oIndexValue2 = mapIndex.Get(oKey2);
            Assert.AreEqual(oExtracted2, oIndexValue2,
                "The index should contain the extracted value for key2.");

            // since the extracted value (21) for key3 fails the filter check
            // LessFilter(extractor, new Integer(15)), it should not be part of
            // the index
            object oIndexValue3 = mapIndex.Get(oKey3);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue3);

            // get the inverse map
            var mapInverse = (SynchronizedDictionary)mapIndex.IndexContents;

            // assert the the inverse map does contain an entry for the
            // extracted values for key
            Assert.IsTrue(mapInverse.Contains(oExtracted));

            // assert that the set mapped to the extracted value for key contains
            // key
            var set = mapInverse[oExtracted] as HashSet;
            Assert.IsNotNull(set);
            Assert.IsTrue(set.Contains(oKey),
                "The index's inverse map should contain the key.");

            // assert the the inverse map does contain an entry for the
            // extracted values for key2
            Assert.IsTrue(mapInverse.Contains(oExtracted2));

            // assert that the set mapped to the extracted value for key2 contains
            // key2
            set = mapInverse[oExtracted2] as HashSet;
            Assert.IsNotNull(set);
            Assert.IsTrue(set.Contains(oKey2),
                "The index's inverse map should contain the key2.");

            // assert the the inverse map does not contain an entry for the
            // extracted value for key3
            Assert.IsFalse(mapInverse.Contains(oExtracted3));

            // update the index
            mapIndex.Update(entryNew);   // key  (extracted value : 11)
            mapIndex.Update(entryNew2);  // key2 (extracted value : 30)

            // assert the the index now contains the updated value for key
            oIndexValue = mapIndex.Get(oKey);
            Assert.AreEqual(oExtractedNew, oIndexValue,
                "The index should contain the updated value for key.");

            // assert that the instance for the extracted value 11 is reused
            Assert.AreSame(oIndexValue, oIndexValue2,
                "The value for key and key2 should be the same instance.");

            // verify the value for key2 is no longer available from the index
            // since the updated extracted value (30) for key2 fails the filter
            // check : LessFilter(extractor, new Integer(15)), it should not be
            // part of the index
            oIndexValue2 = mapIndex.Get(oKey2);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue2,
                "The index should not contain the extracted value for key2.");

            // assert the inverse map does contain an entry for the
            // extracted value for key
            mapInverse = (SynchronizedDictionary)mapIndex.IndexContents;
            Assert.IsTrue(mapInverse.Contains(oExtractedNew));

            // assert that the set mapped to the old extracted value for key
            // no longer contains key... result of update
            set = mapInverse[oExtracted] as HashSet;
            Assert.IsTrue(set == null || !set.Contains(oKey),
                "The index's inverse map should not contain key.");

            // assert that the set mapped to the extracted value for key contains
            // key
            set = mapInverse[oExtractedNew] as HashSet;
            Assert.IsNotNull(set);
            Assert.IsTrue(set.Contains(oKey), "The index's inverse map should contain key.");

            // assert that the set mapped to the old extracted value for key2
            // no longer contains key2... result of update
            set = mapInverse[oExtracted2] as HashSet;
            Assert.IsTrue(set == null || !set.Contains(oKey2),
                "The index's inverse map should not contain key2.");

            // assert the the inverse map does not contain an entry for the new
            // extracted value for key2... fails filter check
            set = mapInverse[oExtractedNew2] as HashSet;
            Assert.IsTrue(set == null || !set.Contains(oKey2),
                "The index's inverse map should not contain key2.");
        }

        /// <summary>
        /// Test update on a ConditionalIndex.  Verify the following :
        /// 1) the index contains the new value for a key after an update
        /// 2) if multiple equivalent values are inserted into the index
        ///    for different keys, verify that only one copy of the value exists in
        ///    the index
        /// 3) extracted values from entries that do not pass the filter test are
        ///    not added to the index
        /// 4) keys are no longer associated with the old extracted values in the
        ///    inverse mapping after the update 
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>
        [Test]
        public void testUpdate_forwardIndexFalse()
        {
            // create the ConditionalIndex to be tested
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new LessFilter(extractor, 15);
            var mapIndex = new ConditionalIndex(filter, extractor, true, null, false);

            // define the keys and values
            const string oKey = "key";
            Object oValue = 0;
            Object oExtracted = 0;
            Object oNewValue = 1;
            Object oExtractedNew = 1;
            const string oKey2 = "key2";
            Object oValue2 = 2;
            Object oExtracted2 = 2;
            const string oKey3 = "key3";
            Object oValue3 = 21;
            Object oNewValue2 = 4;
            Object oExtractedNew2 = 4;
            ICacheEntry entry = new CacheEntry(oKey, oValue);
            ICacheEntry entry2 = new CacheEntry(oKey2, oValue2, oValue);
            ICacheEntry entry3 = new CacheEntry(oKey3, oValue3, oValue2);
            ICacheEntry entryNew = new CacheEntry(oKey, oNewValue, oValue);
            ICacheEntry entryNew2 = new CacheEntry(oKey2, oNewValue2, oValue2);

            // begin test

            // verify that the index does not contain a value for the tested keys
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey2));
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey3));

            // insert into the index
            mapIndex.Insert(entry);  // key, oExtracted
            mapIndex.Insert(entry2); // key, oExtracted2
            mapIndex.Insert(entry3); // key, oExtracted3

            // all gets should return NO_VALUE since forward index is not supported
            // verify no value from the index for key
            object oIndexValue = mapIndex.Get(oKey);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue);

            // verify no value from the index for key2
            object oIndexValue2 = mapIndex.Get(oKey2);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue2);

            // verify no value in the index for key3
            object oIndexValue3 = mapIndex.Get(oKey3);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue3);

            // update the index
            mapIndex.Update(entryNew);   // key, oExtractedNew
            mapIndex.Update(entryNew2);  // key2, oExtractedNew2

            // all gets should return NO_VALUE since forward index is not supported
            // verify no value from the index for key
            oIndexValue = mapIndex.Get(oKey);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue);

            oIndexValue2 = mapIndex.Get(oKey2);
            Assert.AreEqual(ObjectUtils.NO_VALUE, oIndexValue2);

            // get the inverse map
            var mapInverse = (SynchronizedDictionary)mapIndex.IndexContents;

            // get the set of keys from the inverse map keyed by the extracted
            // value for key
            var set = mapInverse[oExtractedNew] as HashSet;
            Assert.IsNotNull(set);

            // verify that the set of keys contains key
            Assert.IsTrue(set.Contains(oKey),
                "The index's inverse map should contain the key.");

            // get the set of keys from the inverse map keyed by the old extracted
            // value for key
            set = (HashSet) mapInverse[oExtracted];

            // verify that the set of keys does not contain key
            Assert.IsTrue(set == null || !set.Contains(oKey),
                "The index's inverse map should not contain the key for the old extracted value.");

            // get the set of keys from the inverse map keyed by the extracted
            // value for key2
            set = mapInverse[oExtractedNew2] as HashSet;
            Assert.IsNotNull(set);

            // verify that the set of keys contains key2
            Assert.IsTrue(set.Contains(oKey2),
                "The index's inverse map should contain the key2.");

            // get the set of keys from the inverse map keyed by the old extracted
            // value for key2
            set = mapInverse[oExtracted2] as HashSet;

            // verify that the set of keys does not contain key2
            Assert.IsTrue(set == null || !set.Contains(oKey2),
                "The index's inverse map should not contain key2 for the old extracted value.");
        }

        /// <summary>
        /// Test delete from a ConditionalIndex.  Verify that the index does not
        /// contain a value for a key after an delete.
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>    
        [Test]
        public void testDelete()
        {
            // create the ConditionalIndex to be tested
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new LessFilter(extractor, 15);
            var mapIndex = new ConditionalIndex(filter, extractor, true, null, true);

            // define the keys and values
            const string oKey       = "key";
            Object       oValue     = 1;
            Object       oExtracted = 1;
            ICacheEntry  entry      = new CacheEntry(oKey, oValue, oValue);

            // begin test

            // verify that the index does not contain a value for the tested keys
            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey));

            // insert into the index
            mapIndex.Insert(entry);

            object extractedValue = mapIndex.Get(oKey);
            Assert.AreEqual(oExtracted, extractedValue,
                "The index should contain the extracted value for key.");

            mapIndex.Delete(entry);

            Assert.AreEqual(ObjectUtils.NO_VALUE, mapIndex.Get(oKey));
        }

        /// <summary>
        /// Test delete from a ConditionalIndex.  Verify that the index does not
        /// contain a value for a key after an delete.
        /// </summary>
        /// <exception cref="Exception">
        /// Rethrow any exception to be caught by test framework.
        /// </exception>        
        [Test]
        public void testDelete_forwardIndexFalse()
        {
            // create the ConditionalIndex to be tested
            IValueExtractor extractor = new IdentityExtractor();
            IFilter filter = new LessFilter(extractor, 15);
            var mapIndex = new ConditionalIndex(filter, extractor, true, null, false);

            // define the keys and values for the mock entries
            const string oKey = "key";
            Object oValue = 1;
            Object oExtracted = 1;
            ICacheEntry entry = new CacheEntry(oKey, oValue);
            ICacheEntry delEntry = new CacheEntry(oKey, oValue, oValue);

            // begin test

            // assert the the inverse map does not contain an entry for the extracted value
            Assert.IsFalse(mapIndex.IndexContents.Contains(oExtracted));

            // insert into the index
            mapIndex.Insert(entry);

            // assert the the inverse map does contain an entry for the extracted value
            Assert.IsTrue(mapIndex.IndexContents.Contains(oExtracted));

            mapIndex.Delete(delEntry);

            // get the inverse map
            var mapInverse = (SynchronizedDictionary)mapIndex.IndexContents;

            // get the set of keys from the inverse map keyed by the extracted
            // value for key
            var set = (HashSet) mapInverse[oExtracted];

            // verify that the set of keys does not contain key
            Assert.IsTrue(set == null || !set.Contains(oKey),
                "The index's inverse map should not contain the key for the extracted value.");
        }

        private static ConditionalIndex createIndex(IDictionary map, IFilter filter, IValueExtractor extractor, bool fSupportForwardMap)
        {
            var index = new ConditionalIndex(filter, extractor, false, null, fSupportForwardMap);
            foreach (DictionaryEntry de in map)
            {
                ICacheEntry entry = new CacheEntry(de.Key, de.Value);
                index.Insert(entry);
            }
            return index;
        }
    }
}